using Microsoft.Extensions.Options;
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

    // Configuración del agente
    builder.Services.Configure<AgentOptions>(
        builder.Configuration.GetSection(AgentOptions.SectionName));

    var agentConfig = builder.Configuration.GetSection(AgentOptions.SectionName);
    var panelPuerto = int.TryParse(agentConfig["PanelPuerto"], out var p) ? p : 5180;

    // El panel de control solo escucha en localhost (no se expone a la red)
    builder.WebHost.UseUrls($"http://localhost:{panelPuerto}");

    // Servicios
    builder.Services.AddSingleton<AgentState>();
    builder.Services.AddSingleton<FirebirdExtractor>();
    builder.Services.AddSingleton<LocalStore>();
    builder.Services.AddSingleton<CycleRunner>();

    // HttpClient para comunicación con el servidor central
    builder.Services.AddHttpClient("servidor-central", client =>
    {
        client.BaseAddress = new Uri(agentConfig["ServerUrl"] ?? "http://localhost:5170");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddSingleton<ServerClient>(sp => new ServerClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("servidor-central"),
        sp.GetRequiredService<IOptions<AgentOptions>>(),
        sp.GetRequiredService<ILogger<ServerClient>>()));

    // Worker principal
    builder.Services.AddHostedService<Worker>();

    // Soporte para ejecución como servicio Windows
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PetrolRios Station Agent";
    });

    var app = builder.Build();

    // Modo inicial según configuración
    var estadoInicial = app.Services.GetRequiredService<AgentState>();
    estadoInicial.ModoAutomatico = agentConfig.GetValue("InicioAutomatico", true);

    // ─────────── Panel de control local del agente ───────────

    app.MapGet("/", () => Results.Content(PanelHtml.Pagina, "text/html; charset=utf-8"));

    app.MapGet("/api/estado", (AgentState state, CycleRunner runner, IOptions<AgentOptions> opts) =>
    {
        var o = opts.Value;
        return Results.Json(new
        {
            estacion = o.CodigoEstacion,
            servidor = o.ServerUrl,
            intervaloSegundos = o.IntervaloSegundos,
            firebird = OcultarPassword(o.FirebirdConnectionString),
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

    app.MapPost("/api/sincronizar", async (CycleRunner runner, AgentState state, CancellationToken ct) =>
    {
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
