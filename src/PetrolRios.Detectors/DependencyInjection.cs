using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules.InvoiceAnomaly;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        // Motor de scoring
        services.AddSingleton<RiskScoringEngine>();

        // Reglas de detección individuales (Strategy por regla). El detector de facturas ya no
        // contiene su lógica: la obtiene de estas reglas inyectadas. Agregar una regla nueva =
        // registrar su clase aquí, sin modificar ningún detector (principio Abierto/Cerrado).
        services.AddSingleton<IDetectionRule, TasaAnulacionesRule>();
        services.AddSingleton<IDetectionRule, PrecioFueraListaRule>();
        services.AddSingleton<IDetectionRule, CamposObligatoriosRule>();
        services.AddSingleton<IDetectionRule, DescuentoExcesivoRule>();
        services.AddSingleton<IDetectionRule, TotalInconsistenteRule>();
        services.AddSingleton<IDetectionRule, FechaFueraDeRangoRule>();
        services.AddSingleton<IDetectionRule, AnulacionRecurrenteRule>();
        services.AddSingleton<IDetectionRule, DespachoNoFacturadoRule>();

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
