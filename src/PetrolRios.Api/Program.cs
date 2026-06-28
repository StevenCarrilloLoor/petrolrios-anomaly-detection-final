using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PetrolRios.Api.Middleware;
using PetrolRios.Application;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors;
using PetrolRios.Infrastructure;
using PetrolRios.Infrastructure.Hubs;
using PetrolRios.Infrastructure.Jobs;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Application.Security;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Secretos locales (SMTP/App Password) — archivo git-ignoreado, opcional.
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

    // Serilog
    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

    // Capas de la aplicación (Clean Architecture)
    builder.Services.AddApplication();

    // Conexión a PostgreSQL flexible y editable sin recompilar: la resuelve ConexionStore con
    // prioridad variable de entorno › config/connection.json › appsettings. La base puede vivir
    // en cualquier máquina o sistema operativo; el central solo necesita la cadena.
    var configDir = Path.Combine(builder.Environment.ContentRootPath, "config");
    var conexionStore = new PetrolRios.Infrastructure.Configuracion.ConexionStore(configDir, builder.Configuration);
    var connectionString = conexionStore.ResolverActiva();

    // Asistente de primer arranque: SOLO en Producción, si no hay base alcanzable, levanta una
    // pantalla de configuración (en vez de caerse) hasta que se guarde una conexión que funcione.
    // En desarrollo/tests no aplica, para no romper el flujo local ni las pruebas de integración.
    if (builder.Environment.IsProduction())
    {
        var alcanzable = !string.IsNullOrWhiteSpace(connectionString)
            && (await conexionStore.ProbarAsync(connectionString!, CancellationToken.None)).Ok;
        if (!alcanzable)
        {
            await PetrolRios.Api.Setup.SetupMode.EjecutarAsync(args, conexionStore);
            connectionString = conexionStore.ResolverActiva();
        }
    }

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "Falta la conexión a PostgreSQL. Configúrela por variable de entorno ConnectionStrings__PostgreSQL, " +
            "el archivo config/connection.json (Ajustes → Conexión a la base) o appsettings.");
    builder.Services.AddInfrastructure(connectionString, builder.Configuration);
    builder.Services.AddSingleton<PetrolRios.Application.Interfaces.IConexionStore>(conexionStore);

    // Parámetros de operación editables sin recompilar (nivel mínimo de correo + cron del job),
    // persistidos en config/operacion.json (Ajustes → Operación del sistema, solo Admin).
    builder.Services.AddSingleton<PetrolRios.Application.Interfaces.IParametrosOperacion>(
        new PetrolRios.Infrastructure.Configuracion.ParametrosOperacionStore(configDir, builder.Configuration));

    builder.Services.AddDetectors();

    // JWT Authentication
    var jwtSecret = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Falta Jwt:SecretKey en la configuración.");

    // Endurecimiento en producción: la clave JWT no puede ser débil ni la de desarrollo.
    if (!builder.Environment.IsDevelopment())
    {
        const string claveDev = "DEV_ONLY_SuperSecretKey_AtLeast32Characters_Long!";
        if (jwtSecret.Length < 32 || jwtSecret == claveDev)
            throw new InvalidOperationException(
                "Jwt:SecretKey de producción inválida. Configure una clave robusta (>=32 caracteres) " +
                "mediante variable de entorno (Jwt__SecretKey) y nunca use la clave de desarrollo.");
    }
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        // Permitir SignalR enviar token por query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Central", policy =>
        {
            policy.RequireAuthenticatedUser();
            // Defensa en profundidad: ni las cuentas ligadas a una estación (claim EstacionId) ni el
            // rol "Agente" (cuenta de servicio del Station Agent) pueden entrar a la app central.
            policy.RequireAssertion(context =>
                !context.User.HasClaim(c => c.Type == PetrolRiosClaimTypes.EstacionId)
                && !context.User.IsInRole("Agente"));
        });
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddFluentValidationAutoValidation();

    // API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "PetrolRíos API",
            Version = "v1",
            Description = "Sistema de detección de anomalías transaccionales"
        });

        // Esquema de seguridad JWT en Swagger
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Ejemplo: 'Bearer {token}'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    builder.Services.AddSignalR();

    // Listener del fan-out de alertas en tiempo real: cada instancia escucha (LISTEN) las
    // notificaciones publicadas por cualquier instancia y las empuja a sus clientes SignalR.
    builder.Services.AddHostedService<PetrolRios.Infrastructure.RealTime.AlertasNotificacionListener>();

    // Hangfire con PostgreSQL storage
    // Hangfire usa su propia conexión si se define; si no, la misma conexión central ya resuelta.
    var hangfireCs = builder.Configuration.GetConnectionString("Hangfire")
        ?? connectionString;
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireCs)));
    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = 1;
    });

    // CORS para frontend React
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetValue<string>("Cors:FrontendUrl") ?? "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Middleware global de excepciones (antes de todo)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // HTTPS/HSTS en producción (configurable). En desarrollo se omite para no romper el flujo local.
    if (!app.Environment.IsDevelopment()
        && builder.Configuration.GetValue("Seguridad:ForzarHttps", true))
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("Frontend");

    // Frontend compilado (wwwroot): en producción la API sirve la SPA — un solo ejecutable
    var sirveFrontend = Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot"))
        && File.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"));
    if (sirveFrontend)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // SignalR hub
    app.MapHub<AlertsHub>("/hubs/alerts");

    // Hangfire dashboard — protegido: solo local o Administrador autenticado.
    var dashboardPath = builder.Configuration.GetValue<string>("Hangfire:DashboardPath") ?? "/hangfire";
    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = new[] { new PetrolRios.Api.Security.HangfireLocalAuthorizationFilter() }
    });

    // Job recurrente de detección de anomalías. La frecuencia (cron) sale de config/operacion.json
    // (editable en Ajustes → Operación del sistema) y, en su defecto, de appsettings.
    var cronExpression = app.Services
        .GetRequiredService<PetrolRios.Application.Interfaces.IParametrosOperacion>()
        .Actual().CronExpression;
    RecurringJob.AddOrUpdate<AnomalyDetectionJob>(
        "anomaly-detection",
        job => job.ExecuteAsync(CancellationToken.None),
        cronExpression);

    // Schedule adaptativo de precios de combustible: "tick" cada hora (al minuto 7). El propio job decide
    // si toca scrapear (modo normal días 1–10 a las 08:00; modo alerta cada hora el 11–12) y aplica el jitter.
    RecurringJob.AddOrUpdate<PetrolRios.Infrastructure.Services.Precios.PreciosCombustibleScheduler>(
        "precios-combustible",
        job => job.TickAsync(CancellationToken.None),
        "7 * * * *");

    if (sirveFrontend)
    {
        // SPA fallback: cualquier ruta no-API devuelve el index del frontend
        app.MapFallbackToFile("index.html");
    }
    else
    {
        app.MapGet("/", () => Results.Ok(new { Status = "PetrolRíos API v1 operativa" }));
    }

    // Migraciones y seed data
    await SeedData.InitializeAsync(app.Services);

    // Autodescubrimiento de relaciones entre tablas (best-effort, no bloquea el arranque): cruza las
    // llaves compartidas entre las fuentes y las valida con el solapamiento de valores en staging, para
    // que el creador de reglas ofrezca campos relacionados sin definir nada a mano.
    try
    {
        using var scope = app.Services.CreateScope();
        var descubridor = scope.ServiceProvider.GetRequiredService<IDescubridorRelaciones>();
        var nuevas = await descubridor.DescubrirAsync(CancellationToken.None);
        if (nuevas > 0) Log.Information("Autodescubridor: {Nuevas} relación(es) entre tablas creada(s).", nuevas);
    }
    catch (Exception exDesc)
    {
        Log.Warning(exDesc, "El autodescubridor de relaciones falló al arrancar; se omite.");
    }

    app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    // HostAbortedException debe propagarse: la lanzan el tooling de EF Core
    // (migraciones) y WebApplicationFactory (tests de integración).
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

// Necesario para WebApplicationFactory en tests de integración
public partial class Program;
