using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Orquestador base de detectores. Ya NO contiene la lógica de cada regla: <b>orquesta</b> las
/// reglas (<see cref="IDetectionRule"/>) de su carril que se le inyectan. Agregar una regla nueva =
/// crear una clase <see cref="IDetectionRule"/> y registrarla en DI, sin tocar ningún detector
/// (principio Abierto/Cerrado). Respeta el on/off de cada regla y su umbral/carril configurados.
///
/// Cada detector concreto solo declara su <see cref="Type"/>; el motor de la corrida (filtrar las
/// reglas de su carril, saltar las desactivadas y acumular las anomalías) vive aquí una sola vez.
/// </summary>
public abstract class RuleBasedDetector : IAnomalyDetector
{
    private readonly IReadOnlyList<IDetectionRule> _reglas;
    private readonly ILogger _logger;

    /// <summary>Carril del detector. Cada subclase devuelve el suyo (un literal del enum).</summary>
    public abstract TipoDetector Type { get; }

    /// <summary>
    /// Recibe TODAS las reglas registradas en DI; en cada corrida se filtran las que pertenecen
    /// a este detector (<see cref="IDetectionRule.Detector"/> == <see cref="Type"/>). El filtro se
    /// hace por corrida —no en el constructor— porque <see cref="Type"/> es virtual.
    /// </summary>
    protected RuleBasedDetector(IEnumerable<IDetectionRule> reglas, ILogger logger)
    {
        _reglas = reglas as IReadOnlyList<IDetectionRule> ?? reglas.ToList();
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();

        foreach (var regla in _reglas)
        {
            if (regla.Detector != Type) continue; // regla de otro detector

            // Configuración persistida de la regla (umbral, carril, activa). Null = aún no está en BD.
            var config = context.Reglas.FirstOrDefault(r => r.ParametroNombre == regla.Parametro);
            if (config is { Activa: false }) continue; // regla desactivada: no se evalúa

            var nuevas = regla.Evaluar(context, config);
            // Si la regla pidió aviso por correo, marcar sus anomalías para que el job lo envíe.
            if (config?.NotificarCorreo == true)
                foreach (var a in nuevas) a.NotificarCorreo = true;
            anomalies.AddRange(nuevas);
        }

        _logger.LogDebug("{Detector}: {Count} anomalías en estación {Est}",
            Type, anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }
}
