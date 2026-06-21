using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías en facturas. Ya NO contiene la lógica de cada regla: <b>orquesta</b> las
/// reglas (<see cref="IDetectionRule"/>) de su carril que se le inyectan. Agregar una regla nueva =
/// crear una clase <see cref="IDetectionRule"/> y registrarla en DI, sin tocar este detector
/// (principio Abierto/Cerrado). Respeta el on/off de cada regla y su umbral/carril configurados.
/// </summary>
public sealed class InvoiceAnomalyDetector : IAnomalyDetector
{
    private readonly IReadOnlyList<IDetectionRule> _reglas;
    private readonly ILogger<InvoiceAnomalyDetector> _logger;

    public TipoDetector Type => TipoDetector.InvoiceAnomaly;

    public InvoiceAnomalyDetector(IEnumerable<IDetectionRule> reglas, ILogger<InvoiceAnomalyDetector> logger)
    {
        _reglas = reglas.Where(r => r.Detector == TipoDetector.InvoiceAnomaly).ToList();
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();

        foreach (var regla in _reglas)
        {
            // Configuración persistida de la regla (umbral, carril, activa). Null = aún no está en BD.
            var config = context.Reglas.FirstOrDefault(r => r.ParametroNombre == regla.Parametro);
            if (config is { Activa: false }) continue; // regla desactivada: no se evalúa
            anomalies.AddRange(regla.Evaluar(context, config));
        }

        _logger.LogDebug("InvoiceAnomalyDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }
}
