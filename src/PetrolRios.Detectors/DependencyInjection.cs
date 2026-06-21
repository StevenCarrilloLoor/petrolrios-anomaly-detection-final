using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        // Motor de scoring (compartido por todas las reglas)
        services.AddSingleton<RiskScoringEngine>();

        // Auto-registro de TODAS las reglas de detección (IDetectionRule) de este ensamblado.
        // → Agregar una regla nueva = crear su clase en Rules/<Detector>/ y nada más: se descubre
        //   y se registra sola, sin tocar este archivo (principio Abierto/Cerrado).
        foreach (var regla in ReglasDelEnsamblado())
            services.AddSingleton(typeof(IDetectionRule), regla);

        // Detectores (Strategy Pattern). Cada uno es un orquestador delgado que ejecuta las
        // reglas de su propio carril (ver RuleBasedDetector).
        services.AddScoped<IAnomalyDetector, CashFraudDetector>();
        services.AddScoped<IAnomalyDetector, InvoiceAnomalyDetector>();
        services.AddScoped<IAnomalyDetector, PaymentFraudDetector>();
        services.AddScoped<IAnomalyDetector, ComplianceViolationDetector>();
        services.AddScoped<IAnomalyDetector, CustomRuleDetector>();

        return services;
    }

    /// <summary>
    /// Todas las clases concretas que implementan <see cref="IDetectionRule"/> en el ensamblado de
    /// detectores. Es el punto único que hace escalable el sistema: las reglas se descubren por
    /// reflexión, así que sumar reglas no obliga a editar la composición de dependencias.
    /// </summary>
    private static IEnumerable<Type> ReglasDelEnsamblado() =>
        typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IDetectionRule).IsAssignableFrom(t));
}
