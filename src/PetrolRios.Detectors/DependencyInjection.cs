using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        // Motor de scoring
        services.AddSingleton<RiskScoringEngine>();

        // 4 detectores (Strategy Pattern) — inyectados como IEnumerable<IAnomalyDetector>
        services.AddScoped<IAnomalyDetector, CashFraudDetector>();
        services.AddScoped<IAnomalyDetector, InvoiceAnomalyDetector>();
        services.AddScoped<IAnomalyDetector, PaymentFraudDetector>();
        services.AddScoped<IAnomalyDetector, ComplianceViolationDetector>();

        return services;
    }
}
