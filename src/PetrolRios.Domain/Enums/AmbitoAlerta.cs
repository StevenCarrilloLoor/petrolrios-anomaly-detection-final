namespace PetrolRios.Domain.Enums;

/// <summary>
/// Ámbito de una alerta: separa los dos carriles del sistema.
/// Es ortogonal al <see cref="NivelRiesgo"/> (una alerta operativa puede ser urgente
/// sin ser una irregularidad, y una de auditoría puede ser de riesgo medio).
/// </summary>
public enum AmbitoAlerta
{
    /// <summary>
    /// Problema operativo de la estación (error honesto): turno sin cerrar, despacho mal
    /// cerrado, campos faltantes. Se notifica en tiempo real al administrador de la estación
    /// y aparece en la pestaña "Problemas de estación". El central también las ve.
    /// </summary>
    Operativa = 1,

    /// <summary>
    /// Posible irregularidad o problema grave a revisar (gineteo, kiting, créditos no
    /// autorizados, cuadre forzado). Queda en la bandeja de auditoría del central; NO se envía
    /// al administrador de la estación. Es una anomalía a auditar, no un fraude confirmado.
    /// </summary>
    Auditoria = 2,

    /// <summary>
    /// Doble carril: la alerta es a la vez problema operativo de la estación Y anomalía a auditar.
    /// Aparece en "Problemas de estación" (y se avisa al administrador de la estación) Y en la
    /// bandeja del central. Útil para reglas que importan a ambos (p. ej. un despacho irregular que
    /// la estación debe corregir y el central debe auditar).
    /// </summary>
    Ambos = 3
}
