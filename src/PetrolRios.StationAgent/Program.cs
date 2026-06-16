using PetrolRios.StationAgent;
using PetrolRios.StationAgent.Configuration;
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
    builder.Services.AddSingleton<ServerClient>(sp => new ServerClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("servidor-central"),
        sp.GetRequiredService<AgentConfigStore>(),
        sp.GetRequiredService<ILogger<ServerClient>>()));

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

    app.MapGet("/", () => Results.Content(PanelHtml.Pagina, "text/html; charset=utf-8"));

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
            PanelPuerto = actual.PanelPuerto
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

    app.MapPost("/api/sincronizar", async (CycleRunner runner, AgentState state, AgentConfigStore cfg, CancellationToken ct) =>
    {
        if (!cfg.Actual.Configurado)
            return Results.Json(new { ok = false, resultado = "Configure el agente antes de sincronizar." });
        state.RegistrarEvento("INFO", "Sincronización manual solicitada desde el panel");
        var resultado = await runner.RunCycleAsync(ct);
        return Results.Json(new { ok = state.UltimoCicloExitoso, resultado });
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
    bool? InicioAutomatico);
