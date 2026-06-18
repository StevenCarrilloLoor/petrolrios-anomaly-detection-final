using PetrolRios.StationAgent;
using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Security;
using PetrolRios.StationAgent.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/agent-.log", rollingInterval: RollingInterval.Day));

    // Defaults de appsettings.json (semilla del primer arranque)
    builder.Services.Configure<AgentOptions>(
        builder.Configuration.GetSection(AgentOptions.SectionName));

    // Store de configuración editable y persistente (fuente única en runtime)
    builder.Services.AddSingleton<AgentConfigStore>();

    var agentConfig = builder.Configuration.GetSection(AgentOptions.SectionName);
    var panelPuerto = int.TryParse(agentConfig["PanelPuerto"], out var p) ? p : 5180;

    // El panel de control solo escucha en localhost (no se expone a la red)
    builder.WebHost.UseUrls($"http://localhost:{panelPuerto}");

    // Servicios
    builder.Services.AddSingleton<AgentState>();
    builder.Services.AddSingleton<FirebirdExtractor>();
    builder.Services.AddSingleton<LocalStore>();
    builder.Services.AddSingleton<CycleRunner>();

    // HttpClient sin BaseAddress fija: ServerClient construye la URL absoluta a
    // partir del config store en cada petición (la URL del servidor es editable).
    builder.Services.AddHttpClient("servidor-central");
    builder.Services.AddHttpClient("actualizaciones");
    builder.Services.AddSingleton<ServerClient>(sp => new ServerClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("servidor-central"),
        sp.GetRequiredService<AgentConfigStore>(),
        sp.GetRequiredService<ILogger<ServerClient>>()));

    // Servicio de actualización remota (control de versiones)
    builder.Services.AddSingleton<UpdateService>();

    // Autenticación del panel local (RBAC contra el central + respaldo local)
    builder.Services.AddSingleton<PanelAuth>();

    // Worker principal
    builder.Services.AddHostedService<Worker>();

    // Soporte para ejecución como servicio Windows
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PetrolRios Station Agent";
    });

    var app = builder.Build();

    // Modo inicial: si no está configurado, arranca en MANUAL (no sincroniza solo
    // hasta que el operador complete y guarde la configuración desde la interfaz).
    var configStore = app.Services.GetRequiredService<AgentConfigStore>();
    var estadoInicial = app.Services.GetRequiredService<AgentState>();
    var inicial = configStore.Actual;
    estadoInicial.ModoAutomatico = inicial.Configurado && inicial.InicioAutomatico;

    // ─────────── Panel de control local del agente ───────────

    // Middleware: protege todos los /api/* salvo los de autenticación. Sin sesión
    // válida no se puede ver ni cambiar nada del agente (cierra el hueco de que
    // cualquiera en la máquina lo reconfigure).
    app.Use(async (ctx, next) =>
    {
        var path = ctx.Request.Path;
        if (path.StartsWithSegments("/api"))
        {
            var publica = path.StartsWithSegments("/api/login")
                || path.StartsWithSegments("/api/logout")
                || path.StartsWithSegments("/api/sesion");
            if (!publica)
            {
                // El login del panel es OPT-IN: solo se exige si un administrador lo
                // activó (RequiereLoginPanel). Por defecto el panel está abierto (solo
                // localhost) para poder configurar y conectar el agente sin fricción en
                // el primer despliegue, incluso antes de saber la URL del central.
                var cfg = ctx.RequestServices.GetRequiredService<AgentConfigStore>();
                if (cfg.Actual.RequiereLoginPanel)
                {
                    var auth = ctx.RequestServices.GetRequiredService<PanelAuth>();
                    if (auth.Validar(ctx.Request.Cookies[PanelAuth.CookieNombre]) is null)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await ctx.Response.WriteAsJsonAsync(new { ok = false, mensaje = "Inicie sesión para administrar el agente." });
                        return;
                    }
                }
            }
        }
        await next();
    });

    app.MapGet("/", () => Results.Content(PanelHtml.Pagina, "text/html; charset=utf-8"));

    // ─────────── Autenticación del panel (RBAC) ───────────

    // Helper: valida la cookie de sesión del panel.
    static PanelAuth.Sesion? SesionDe(HttpContext ctx, PanelAuth auth) =>
        auth.Validar(ctx.Request.Cookies[PanelAuth.CookieNombre]);

    app.MapGet("/api/sesion", (HttpContext ctx, PanelAuth auth, AgentConfigStore cfg) =>
    {
        var s = SesionDe(ctx, auth);
        return Results.Json(new
        {
            autenticado = s is not null,
            usuario = s?.Usuario,
            rol = s?.Rol,
            // Si el login no está activado, el panel está abierto (solo localhost).
            requiereLogin = cfg.Actual.RequiereLoginPanel
        });
    });

    app.MapPost("/api/login", async (LoginPanelRequest req, HttpContext ctx, PanelAuth auth, CancellationToken ct) =>
    {
        var (ok, token, rol, mensaje) = await auth.LoginAsync(req.Usuario ?? "", req.Password ?? "", ct);
        if (!ok)
            return Results.Json(new { ok = false, mensaje });

        ctx.Response.Cookies.Append(PanelAuth.CookieNombre, token!, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });
        return Results.Json(new { ok = true, rol, mensaje });
    });

    app.MapPost("/api/logout", (HttpContext ctx, PanelAuth auth) =>
    {
        auth.Cerrar(ctx.Request.Cookies[PanelAuth.CookieNombre]);
        ctx.Response.Cookies.Delete(PanelAuth.CookieNombre);
        return Results.Json(new { ok = true });
    });

    app.MapGet("/api/estado", (AgentState state, CycleRunner runner, AgentConfigStore cfg) =>
    {
        var s = cfg.Actual;
        return Results.Json(new
        {
            estacion = s.CodigoEstacion,
            nombreEstacion = s.NombreEstacion,
            zonaEstacion = s.ZonaEstacion,
            servidor = s.ServerUrl,
            intervaloSegundos = s.IntervaloSegundos,
            firebird = OcultarPassword(s.ConstruirFirebirdConnectionString()),
            configurado = s.Configurado,
            modoAutomatico = state.ModoAutomatico,
            inicioAgente = state.InicioAgente,
            uptimeSegundos = Math.Round((DateTime.UtcNow - state.InicioAgente).TotalSeconds),
            watermark = runner.Watermark,
            ultimoCiclo = state.UltimoCiclo,
            ultimoResultado = state.UltimoResultado,
            ultimoCicloExitoso = state.UltimoCicloExitoso,
            ciclosEjecutados = state.CiclosEjecutados,
            totalTransaccionesEnviadas = state.TotalTransaccionesEnviadas,
            lotesPendientes = runner.ContarPendientes(),
            ultimaConexionServidor = state.UltimaConexionServidor,
            ultimaDesconexionServidor = state.UltimaDesconexionServidor,
            ultimaLatenciaServidorMs = state.UltimaLatenciaServidorMs,
            versionAgente = VersionAgente.Actual,
            actualizacionDisponible = state.ActualizacionDisponible,
            versionDisponible = state.VersionDisponible,
            notasActualizacion = state.NotasActualizacion,
            actualizacionObligatoria = state.ActualizacionObligatoria,
            aplicandoActualizacion = state.AplicandoActualizacion,
            eventos = state.Eventos
        });
    });

    // Configuración completa (para el formulario de la interfaz; sin password en claro)
    app.MapGet("/api/config", (AgentConfigStore cfg) =>
    {
        var s = cfg.Actual;
        return Results.Json(new
        {
            s.CodigoEstacion,
            s.NombreEstacion,
            s.ZonaEstacion,
            s.ServerUrl,
            s.Email,
            tienePassword = !string.IsNullOrEmpty(s.Password),
            s.ServerTimeoutSegundos,
            s.FirebirdHost,
            s.FirebirdPort,
            s.FirebirdDatabase,
            s.FirebirdUser,
            tieneFirebirdPassword = !string.IsNullOrEmpty(s.FirebirdPassword),
            s.FirebirdCharset,
            s.FirebirdDialect,
            s.FirebirdWireCrypt,
            s.IntervaloSegundos,
            s.InicioAutomatico,
            s.UpdateFeedUrl,
            s.UpdateFeedFallbackUrl,
            s.RequiereLoginPanel,
            s.PanelLocalUsuario,
            tienePanelLocalPassword = !string.IsNullOrEmpty(s.PanelLocalPasswordHash),
            s.Configurado
        });
    });

    // Guardar configuración. Los campos de password vacíos conservan el valor actual.
    app.MapPost("/api/config", (GuardarConfigRequest req, AgentConfigStore cfg, AgentState state) =>
    {
        var actual = cfg.Actual;
        var nueva = new AgentSettings
        {
            CodigoEstacion = (req.CodigoEstacion ?? actual.CodigoEstacion).Trim().ToUpperInvariant(),
            NombreEstacion = (req.NombreEstacion ?? actual.NombreEstacion).Trim(),
            ZonaEstacion = (req.ZonaEstacion ?? actual.ZonaEstacion).Trim(),
            ServerUrl = (req.ServerUrl ?? actual.ServerUrl).Trim(),
            Email = (req.Email ?? actual.Email).Trim(),
            Password = string.IsNullOrEmpty(req.Password) ? actual.Password : req.Password,
            ServerTimeoutSegundos = req.ServerTimeoutSegundos ?? actual.ServerTimeoutSegundos,
            FirebirdHost = (req.FirebirdHost ?? actual.FirebirdHost).Trim(),
            FirebirdPort = req.FirebirdPort ?? actual.FirebirdPort,
            FirebirdDatabase = (req.FirebirdDatabase ?? actual.FirebirdDatabase).Trim(),
            FirebirdUser = (req.FirebirdUser ?? actual.FirebirdUser).Trim(),
            FirebirdPassword = string.IsNullOrEmpty(req.FirebirdPassword) ? actual.FirebirdPassword : req.FirebirdPassword,
            FirebirdCharset = (req.FirebirdCharset ?? actual.FirebirdCharset).Trim(),
            FirebirdDialect = req.FirebirdDialect ?? actual.FirebirdDialect,
            FirebirdWireCrypt = (req.FirebirdWireCrypt ?? actual.FirebirdWireCrypt).Trim(),
            IntervaloSegundos = req.IntervaloSegundos ?? actual.IntervaloSegundos,
            InicioAutomatico = req.InicioAutomatico ?? actual.InicioAutomatico,
            LocalStorePath = actual.LocalStorePath,
            PanelPuerto = actual.PanelPuerto,
            UpdateFeedUrl = (req.UpdateFeedUrl ?? actual.UpdateFeedUrl).Trim(),
            UpdateFeedFallbackUrl = (req.UpdateFeedFallbackUrl ?? actual.UpdateFeedFallbackUrl).Trim(),
            NombreServicioWindows = actual.NombreServicioWindows,
            FuentesExtraccion = actual.FuentesExtraccion,
            RequiereLoginPanel = req.RequiereLoginPanel ?? actual.RequiereLoginPanel,
            PanelLocalUsuario = string.IsNullOrWhiteSpace(req.PanelLocalUsuario)
                ? actual.PanelLocalUsuario : req.PanelLocalUsuario.Trim(),
            // Si llega una contraseña local nueva, se guarda como hash PBKDF2 (nunca en claro).
            PanelLocalPasswordHash = string.IsNullOrEmpty(req.PanelLocalPassword)
                ? actual.PanelLocalPasswordHash
                : PetrolRios.StationAgent.Security.PasswordHasher.Hash(req.PanelLocalPassword)
        };

        if (string.IsNullOrWhiteSpace(nueva.CodigoEstacion))
            return Results.BadRequest(new { mensaje = "El código de estación es obligatorio." });
        if (string.IsNullOrWhiteSpace(nueva.NombreEstacion))
            return Results.BadRequest(new { mensaje = "El nombre de la estación es obligatorio." });

        cfg.Guardar(nueva);
        state.ModoAutomatico = nueva.InicioAutomatico;
        state.RegistrarEvento("OK", $"Configuración guardada — estación '{nueva.NombreEstacion}' ({nueva.CodigoEstacion})");
        return Results.Json(new { ok = true });
    });

    // ─── Fuentes de extracción configurables (multi-tabla) ───
    app.MapGet("/api/fuentes", (AgentConfigStore cfg) =>
        Results.Json(new { ok = true, fuentes = cfg.Actual.FuentesExtraccion }));

    app.MapPost("/api/fuentes", async (
        GuardarFuentesRequest req, AgentConfigStore cfg, FirebirdExtractor extractor,
        AgentState state, CancellationToken ct) =>
    {
        var fuentes = (req.Fuentes ?? new())
            .Where(f => !string.IsNullOrWhiteSpace(f.Tabla))
            .Select(f => new FuenteExtraccion
            {
                Nombre = (f.Nombre ?? "").Trim(),
                Tabla = (f.Tabla ?? "").Trim().ToUpperInvariant(),
                ColumnaWatermark = string.IsNullOrWhiteSpace(f.ColumnaWatermark) ? null : f.ColumnaWatermark.Trim(),
                Activa = f.Activa ?? true
            })
            .ToList();

        // Validar que las tablas existan contra el catálogo real (solo lectura).
        try
        {
            var tablas = await extractor.ListarTablasAsync(ct);
            var faltan = fuentes
                .Where(f => !tablas.Any(t => t.Equals(f.Tabla, StringComparison.OrdinalIgnoreCase)))
                .Select(f => f.Tabla)
                .ToList();
            if (faltan.Count > 0)
                return Results.Json(new { ok = false, mensaje = $"Estas tablas no existen en la base: {string.Join(", ", faltan)}" });
        }
        catch (Exception ex)
        {
            return Results.Json(new { ok = false, mensaje = $"No se pudo validar contra Firebird: {ex.Message}" });
        }

        var nueva = cfg.Actual.Clonar();
        nueva.FuentesExtraccion = fuentes;
        cfg.Guardar(nueva);
        state.RegistrarEvento("OK", $"Fuentes de extracción guardadas ({fuentes.Count})");
        return Results.Json(new { ok = true });
    });

    app.MapPost("/api/sincronizar", async (CycleRunner runner, AgentState state, AgentConfigStore cfg, CancellationToken ct) =>
    {
        if (!cfg.Actual.Configurado)
            return Results.Json(new { ok = false, resultado = "Configure el agente antes de sincronizar." });
        state.RegistrarEvento("INFO", "Sincronización manual solicitada desde el panel");
        var resultado = await runner.RunCycleAsync(ct);
        return Results.Json(new { ok = state.UltimoCicloExitoso, resultado });
    });

    // Re-sincronizar: reinicia la marca de agua a una fecha para reenviar datos.
    app.MapPost("/api/reiniciar-watermark", (ReiniciarWatermarkRequest req, CycleRunner runner) =>
    {
        if (!DateTime.TryParse(req.Fecha, out var fecha))
            return Results.Json(new { ok = false, mensaje = "Fecha inválida." });
        runner.ReiniciarWatermark(fecha.ToUniversalTime());
        return Results.Json(new { ok = true, mensaje = $"Marca de agua reiniciada a {fecha:yyyy-MM-dd HH:mm}. Los datos se reenviarán en el próximo ciclo." });
    });

    app.MapPost("/api/modo", (ModoRequest request, AgentState state) =>
    {
        state.ModoAutomatico = request.Automatico;
        state.RegistrarEvento("INFO",
            request.Automatico
                ? "Modo AUTOMÁTICO activado — el agente sincroniza solo"
                : "Modo MANUAL activado — sincronice desde el panel");
        return Results.Json(new { ok = true, modoAutomatico = state.ModoAutomatico });
    });

    app.MapPost("/api/probar-firebird", async (FirebirdExtractor extractor, AgentState state, CancellationToken ct) =>
    {
        var (ok, mensaje, total) = await extractor.ProbarConexionAsync(ct);
        state.RegistrarEvento(ok ? "OK" : "ERROR", $"Prueba Firebird: {mensaje}");
        return Results.Json(new { ok, mensaje, totalDocumentos = total });
    });

    // Explorador de esquema: lista de tablas de la base Firebird (auto-documentación).
    app.MapGet("/api/firebird/tablas", async (FirebirdExtractor extractor, CancellationToken ct) =>
    {
        try { return Results.Json(new { ok = true, tablas = await extractor.ListarTablasAsync(ct) }); }
        catch (Exception ex) { return Results.Json(new { ok = false, mensaje = ex.Message }); }
    });

    // Descripción (campos y tipos) de una tabla; verifica que exista.
    app.MapGet("/api/firebird/tabla/{nombre}", async (string nombre, FirebirdExtractor extractor, CancellationToken ct) =>
    {
        try
        {
            var desc = await extractor.DescribirTablaAsync(nombre, ct);
            return desc.Existe
                ? Results.Json(new { ok = true, desc })
                : Results.Json(new { ok = false, mensaje = $"La tabla '{nombre}' no existe en esta base." });
        }
        catch (Exception ex) { return Results.Json(new { ok = false, mensaje = ex.Message }); }
    });

    app.MapPost("/api/probar-servidor", async (ServerClient client, AgentState state, CancellationToken ct) =>
    {
        var (ok, mensaje, latencia) = await client.ProbarConexionAsync(ct);
        if (ok)
        {
            state.UltimaConexionServidor = DateTime.UtcNow;
            state.UltimaLatenciaServidorMs = latencia;
        }
        else
        {
            state.UltimaDesconexionServidor = DateTime.UtcNow;
        }
        state.RegistrarEvento(ok ? "OK" : "ERROR", $"Prueba servidor: {mensaje}");
        return Results.Json(new { ok, mensaje, latenciaMs = latencia });
    });

    // Revisar manualmente el feed de actualización (botón "Buscar actualización")
    app.MapPost("/api/revisar-actualizacion", async (UpdateService updater, AgentState state, CancellationToken ct) =>
    {
        var manifiesto = await updater.ConsultarAsync(ct);
        if (manifiesto is null)
            return Results.Json(new { ok = false, mensaje = "No se pudo contactar el feed de actualización." });

        var hay = UpdateService.EsMasNueva(manifiesto.Version, VersionAgente.Actual);
        state.ActualizacionDisponible = hay;
        state.VersionDisponible = hay ? manifiesto.Version : null;
        state.NotasActualizacion = hay ? manifiesto.Notas : null;
        state.UrlActualizacion = hay ? manifiesto.Url : null;
        state.Sha256Actualizacion = hay ? manifiesto.Sha256 : null;
        state.ActualizacionObligatoria = hay && manifiesto.Obligatoria;

        return Results.Json(new
        {
            ok = true,
            hayActualizacion = hay,
            versionInstalada = VersionAgente.Actual,
            versionDisponible = manifiesto.Version,
            mensaje = hay
                ? $"Actualización {manifiesto.Version} disponible."
                : $"El agente está al día (versión {VersionAgente.Actual})."
        });
    });

    // Aplicar la actualización con un clic: descarga, verifica checksum, reemplaza y reinicia
    app.MapPost("/api/actualizar", async (
        UpdateService updater, AgentState state, IHostApplicationLifetime vida, CancellationToken ct) =>
    {
        if (!state.ActualizacionDisponible || string.IsNullOrWhiteSpace(state.UrlActualizacion))
            return Results.Json(new { ok = false, mensaje = "No hay ninguna actualización pendiente." });
        if (state.AplicandoActualizacion)
            return Results.Json(new { ok = false, mensaje = "Ya hay una actualización en curso." });

        state.AplicandoActualizacion = true;
        var manifiesto = new ManifiestoActualizacion
        {
            Version = state.VersionDisponible ?? "",
            Url = state.UrlActualizacion!,
            Sha256 = state.Sha256Actualizacion,
            Notas = state.NotasActualizacion
        };

        var (ok, mensaje) = await updater.AplicarAsync(manifiesto, ct);
        state.RegistrarEvento(ok ? "OK" : "ERROR", $"Actualización: {mensaje}");

        if (ok)
        {
            // Dar tiempo a responder al panel antes de cerrar para liberar el .exe
            _ = Task.Run(async () => { await Task.Delay(1500); vida.StopApplication(); });
        }
        else
        {
            state.AplicandoActualizacion = false;
        }
        return Results.Json(new { ok, mensaje });
    });

    Log.Information("Panel del agente disponible en http://localhost:{Puerto}", panelPuerto);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El agente falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

static string OcultarPassword(string connectionString) =>
    System.Text.RegularExpressions.Regex.Replace(
        connectionString, @"(?i)(password\s*=\s*)[^;]+", "$1•••••");

internal sealed record ModoRequest(bool Automatico);

internal sealed record LoginPanelRequest(string? Usuario, string? Password);

internal sealed record ReiniciarWatermarkRequest(string? Fecha);

internal sealed record GuardarFuentesRequest(List<FuenteItem>? Fuentes);
internal sealed record FuenteItem(string? Nombre, string? Tabla, string? ColumnaWatermark, bool? Activa);

internal sealed record GuardarConfigRequest(
    string? CodigoEstacion,
    string? NombreEstacion,
    string? ZonaEstacion,
    string? ServerUrl,
    string? Email,
    string? Password,
    int? ServerTimeoutSegundos,
    string? FirebirdHost,
    int? FirebirdPort,
    string? FirebirdDatabase,
    string? FirebirdUser,
    string? FirebirdPassword,
    string? FirebirdCharset,
    int? FirebirdDialect,
    string? FirebirdWireCrypt,
    int? IntervaloSegundos,
    bool? InicioAutomatico,
    string? UpdateFeedUrl,
    string? UpdateFeedFallbackUrl,
    bool? RequiereLoginPanel,
    string? PanelLocalUsuario,
    string? PanelLocalPassword);
