using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Una regla de detección individual (Strategy a nivel de regla). Cada regla integrada del motor
/// es su propia clase, de modo que <b>agregar una regla nueva es crear una clase y registrarla en
/// DI, sin modificar ningún detector existente</b> (principio Abierto/Cerrado). Los detectores
/// (<see cref="IAnomalyDetector"/>) dejan de contener la lógica y solo orquestan las reglas de su carril.
/// </summary>
public interface IDetectionRule
{
    /// <summary>Detector (carril) al que pertenece la regla.</summary>
    TipoDetector Detector { get; }

    /// <summary>Clave del parámetro configurable; coincide con <see cref="ReglaDeteccion.ParametroNombre"/>.</summary>
    string Parametro { get; }

    /// <summary>Umbral por defecto cuando la regla aún no está parametrizada en la base.</summary>
    double UmbralPorDefecto { get; }

    /// <summary>Carril por defecto (editable después desde el panel de Reglas).</summary>
    AmbitoAlerta AmbitoPorDefecto { get; }

    /// <summary>
    /// Evalúa la regla sobre el contexto del ciclo. <paramref name="regla"/> es la configuración
    /// persistida (umbral y carril vigentes) o <c>null</c> si la regla aún no existe en la base.
    /// </summary>
    IEnumerable<DetectedAnomaly> Evaluar(DetectionContext contexto, ReglaDeteccion? regla);
}
