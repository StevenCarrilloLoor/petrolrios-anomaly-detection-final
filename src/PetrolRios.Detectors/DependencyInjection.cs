using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        // Motor de scoring
        services.AddSingleton<RiskScoringEngine>();

        // Detectores (Strategy Pattern) — inyectados como IEnumerable<IAnomalyDetector>:
        // 4 detectores del motor + el detector genérico de reglas personalizadas
        services.AddScoped<IAnomalyDetector, CashFraudDetector>();
        services.AddScoped<IAnomalyDetector, InvoiceAnomalyDetector>();
        services.AddScoped<IAnomalyDetector, PaymentFraudDetector>();
        services.AddScoped<IAnomalyDetector, ComplianceViolationDetector>();
        services.AddScoped<IAnomalyDetector, CustomRuleDetector>();

        return services;
    }
}
