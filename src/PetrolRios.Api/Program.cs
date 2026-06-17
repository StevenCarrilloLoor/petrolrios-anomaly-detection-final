using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PetrolRios.Api.Middleware;
using PetrolRios.Application;
using PetrolRios.Detectors;
using PetrolRios.Infrastructure;
using PetrolRios.Infrastructure.Hubs;
using PetrolRios.Infrastructure.Jobs;
using PetrolRios.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

    // Capas de la aplicación (Clean Architecture)
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(
        builder.Configuration.GetConnectionString("PostgreSQL")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:PostgreSQL en la configuración."),
        builder.Configuration);
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
    builder.Services.AddAuthorization();

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

    // Hangfire con PostgreSQL storage
    var hangfireCs = builder.Configuration.GetConnectionString("Hangfire")
        ?? builder.Configuration.GetConnectionString("PostgreSQL")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Hangfire en la configuración.");
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

    // Job recurrente de detección de anomalías
    var cronExpression = builder.Configuration.GetValue<string>("Hangfire:CronExpression") ?? "*/5 * * * *";
    RecurringJob.AddOrUpdate<AnomalyDetectionJob>(
        "anomaly-detection",
        job => job.ExecuteAsync(CancellationToken.None),
        cronExpression);

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
