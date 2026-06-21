using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules.CashFraud;
using PetrolRios.Detectors.Rules.ComplianceViolation;
using PetrolRios.Detectors.Rules.InvoiceAnomaly;
using PetrolRios.Detectors.Rules.PaymentFraud;

namespace PetrolRios.Detectors;

public static class DependencyInjection
{
    public static IServiceCollection AddDetectors(this IServiceCollection services)
    {
        // Motor de scoring
        services.AddSingleton<RiskScoringEngine>();

        // Reglas de detección individuales (Strategy por regla). Los detectores ya no contienen su
        // lógica: la obtienen de estas reglas inyectadas. Agregar una regla nueva = registrar su
        // clase aquí, sin modificar ningún detector (principio Abierto/Cerrado).

        // Anomalías de factura
        services.AddSingleton<IDetectionRule, TasaAnulacionesRule>();
        services.AddSingleton<IDetectionRule, PrecioFueraListaRule>();
        services.AddSingleton<IDetectionRule, CamposObligatoriosRule>();
        services.AddSingleton<IDetectionRule, DescuentoExcesivoRule>();
        services.AddSingleton<IDetectionRule, TotalInconsistenteRule>();
        services.AddSingleton<IDetectionRule, FechaFueraDeRangoRule>();
        services.AddSingleton<IDetectionRule, AnulacionRecurrenteRule>();
        services.AddSingleton<IDetectionRule, DespachoNoFacturadoRule>();

        // Anomalías de efectivo
        services.AddSingleton<IDetectionRule, DiferenciaEfectivoRule>();
        services.AddSingleton<IDetectionRule, FaltantesRecurrentesRule>();
        services.AddSingleton<IDetectionRule, CreditoSinClienteRule>();
        services.AddSingleton<IDetectionRule, EfectivoCorporativoRule>();
        services.AddSingleton<IDetectionRule, TurnoSinCerrarRule>();

        // Anomalías de pago
        services.AddSingleton<IDetectionRule, ReversionTardiaRule>();
        services.AddSingleton<IDetectionRule, CreditoSinAutorizacionRule>();
        services.AddSingleton<IDetectionRule, CreditoSinGaranteRule>();
        services.AddSingleton<IDetectionRule, TransaccionesDuplicadasRule>();
        services.AddSingleton<IDetectionRule, DespachosRapidosRule>();

        // Violaciones de cumplimiento
        services.AddSingleton<IDetectionRule, PlacaGenericaRule>();
        services.AddSingleton<IDetectionRule, MultipleCombustibleRule>();
        services.AddSingleton<IDetectionRule, VentaSinPlacaRule>();
        services.AddSingleton<IDetectionRule, FueraHorarioRule>();
        services.AddSingleton<IDetectionRule, VentaSinIdentificacionRule>();
        services.AddSingleton<IDetectionRule, AltoVolumenSinPlacaRule>();

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
