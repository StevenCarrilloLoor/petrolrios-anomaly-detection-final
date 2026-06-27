using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;
using PetrolRios.Infrastructure.Firebird;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Persistence.Repositories;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<PetrolRiosDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositorios y Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAlertaRepository, AlertaRepository>();
        services.AddScoped<IEstacionRepository, EstacionRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IReglaDeteccionRepository, ReglaDeteccionRepository>();

        // Firebird (solo lectura)
        services.Configure<FirebirdOptions>(
            configuration.GetSection(FirebirdOptions.SectionName));
        services.AddSingleton<IFirebirdSourceClientFactory, FirebirdSourceClientFactory>();

        // ETL
        services.AddScoped<IEtlOrchestrator, EtlOrchestrator>();

        // Job de detección
        services.AddScoped<Jobs.AnomalyDetectionJob>();
        services.AddScoped<Services.CuadreLiquidacionService>(); // cuadre de liquidación de turno (#3)

        // Tiempo real entre instancias: fan-out de alertas por PostgreSQL LISTEN/NOTIFY.
        // (El listener BackgroundService se registra en Program.cs, donde está el hosting.)
        services.AddSingleton<IAlertaBroadcaster, RealTime.PostgresAlertaBroadcaster>();

        // Servicios de aplicación
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReglaService, ReglaService>();
        services.AddScoped<IReglaBacktestService, ReglaBacktestService>();
        services.AddScoped<IDescubridorRelaciones, DescubridorRelacionesService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IIngestaService, IngestaService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IMonitoreoService, MonitoreoService>();
        services.AddScoped<IEmpleadoDirectorio, EmpleadoDirectorio>();
        services.AddScoped<IEmailNotificacionService, EmailNotificacionService>();
        services.AddSingleton<QrLoginService>(); // estado en memoria del login por QR
        services.AddSingleton<PasswordResetService>(); // tokens de recuperacion en memoria
        services.AddSingleton<AccountUnlockService>(); // tokens de desbloqueo de cuenta en memoria
        services.AddSingleton<ISolicitudesEsquema, SolicitudesEsquema>(); // solicitudes de "cargar esquema" por estacion
        services.AddSingleton<IConsultasFirebird, Services.ConsultasFirebird>(); // cola de consultas en vivo a Firebird

        return services;
    }
}
