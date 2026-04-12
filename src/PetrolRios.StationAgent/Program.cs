using PetrolRios.StationAgent;
using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Serilog
    builder.Services.AddSerilog((services, config) => config
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/agent-.log", rollingInterval: RollingInterval.Day));

    // Configuración del agente
    builder.Services.Configure<AgentOptions>(
        builder.Configuration.GetSection(AgentOptions.SectionName));

    // Servicios
    builder.Services.AddSingleton<FirebirdExtractor>();
    builder.Services.AddSingleton<LocalStore>();

    // HttpClient para comunicación con el servidor central
    builder.Services.AddHttpClient<ServerClient>((sp, client) =>
    {
        var config = builder.Configuration.GetSection(AgentOptions.SectionName);
        client.BaseAddress = new Uri(config["ServerUrl"] ?? "http://localhost:5000");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    // Worker principal
    builder.Services.AddHostedService<Worker>();

    // Soporte para ejecución como servicio Windows
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PetrolRios Station Agent";
    });

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El agente falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
