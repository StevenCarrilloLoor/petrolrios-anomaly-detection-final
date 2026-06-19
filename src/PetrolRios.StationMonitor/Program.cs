using PetrolRios.StationMonitor;
using PetrolRios.StationMonitor.Configuration;
using PetrolRios.StationMonitor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/monitor-.log", rollingInterval: RollingInterval.Day));

    var panelPuerto = builder.Configuration.GetValue(
        $"{MonitorSettings.SectionName}:PanelPuerto",
        5190);
    builder.WebHost.UseUrls($"http://localhost:{panelPuerto}");

    builder.Services.AddSingleton<MonitorConfigStore>();
    builder.Services.AddSingleton<MonitorState>();
    builder.Services.AddHttpClient("servidor-central", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddSingleton<CentralApiClient>(sp => new CentralApiClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("servidor-central"),
        sp.GetRequiredService<MonitorConfigStore>(),
        sp.GetRequiredService<ILogger<CentralApiClient>>()));
    builder.Services.AddSingleton<ProblemPollingWorker>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<ProblemPollingWorker>());
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PetrolRios Station Monitor";
    });

    var app = builder.Build();

    app.MapGet("/", () => Results.Content(PanelHtml.Pagina, "text/html; charset=utf-8"));

    app.MapGet("/api/estado", (MonitorConfigStore config, MonitorState state) =>
    {
        var settings = config.Actual;
        return Results.Json(state.Snapshot(settings.Configurado, settings.CodigoEstacion));
    });

    app.MapGet("/api/config", (MonitorConfigStore config) =>
    {
        var settings = config.Actual;
        return Results.Json(new
        {
            settings.CodigoEstacion,
            settings.ServerUrl,
            settings.Email,
            TienePassword = !string.IsNullOrWhiteSpace(settings.Password),
            settings.IntervaloSegundos,
            settings.DiasConsulta,
            settings.PanelPuerto,
            settings.Configurado
        });
    });

    app.MapPost("/api/config", async (
        GuardarConfigRequest request,
        MonitorConfigStore config,
        CentralApiClient client,
        MonitorState state,
        ProblemPollingWorker worker,
        CancellationToken ct) =>
    {
        var actual = config.Actual;
        var settings = new MonitorSettings
        {
            CodigoEstacion = (request.CodigoEstacion ?? actual.CodigoEstacion).Trim(),
            ServerUrl = (request.ServerUrl ?? actual.ServerUrl).Trim(),
            Email = (request.Email ?? actual.Email).Trim(),
            Password = string.IsNullOrWhiteSpace(request.Password)
                ? actual.Password
                : request.Password,
            IntervaloSegundos = request.IntervaloSegundos ?? actual.IntervaloSegundos,
            DiasConsulta = request.DiasConsulta ?? actual.DiasConsulta,
            PanelPuerto = actual.PanelPuerto,
            Configurado = true
        };

        if (string.IsNullOrWhiteSpace(settings.CodigoEstacion)
            || string.IsNullOrWhiteSpace(settings.ServerUrl)
            || string.IsNullOrWhiteSpace(settings.Email)
            || string.IsNullOrWhiteSpace(settings.Password))
        {
            return Results.BadRequest(new
            {
                mensaje = "Código de estación, servidor, email y contraseña son obligatorios."
            });
        }

        if (!Uri.TryCreate(settings.ServerUrl, UriKind.Absolute, out _))
            return Results.BadRequest(new { mensaje = "La URL del servidor central no es válida." });

        config.Guardar(settings);
        client.ReiniciarAutenticacion();
        state.RegistrarEvento("INFO", "Configuración guardada. Verificando acceso con el central.");
        var resultado = await worker.RefrescarAhoraAsync(ct);

        return resultado.Ok
            ? Results.Json(new { ok = true, mensaje = resultado.Mensaje })
            : Results.BadRequest(new { ok = false, mensaje = resultado.Mensaje });
    });

    app.MapPost("/api/actualizar", async (
        ProblemPollingWorker worker,
        CancellationToken ct) =>
    {
        var resultado = await worker.RefrescarAhoraAsync(ct);
        return Results.Json(new { ok = resultado.Ok, mensaje = resultado.Mensaje });
    });

    app.MapPost("/api/probar", async (
        CentralApiClient client,
        MonitorState state,
        CancellationToken ct) =>
    {
        var resultado = await client.ProbarAsync(ct);
        state.RegistrarEvento(resultado.Ok ? "OK" : "ERROR", resultado.Mensaje);
        return Results.Json(new { ok = resultado.Ok, mensaje = resultado.Mensaje });
    });

    Log.Information(
        "Monitor de estación disponible en http://localhost:{Puerto}",
        panelPuerto);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El Monitor de estación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

internal sealed record GuardarConfigRequest(
    string? CodigoEstacion,
    string? ServerUrl,
    string? Email,
    string? Password,
    int? IntervaloSegundos,
    int? DiasConsulta);
