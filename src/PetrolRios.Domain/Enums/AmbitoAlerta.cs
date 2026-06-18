namespace PetrolRios.Domain.Enums;

/// <summary>
/// Ámbito de una alerta: separa los dos carriles del sistema.
/// Es ortogonal al <see cref="NivelRiesgo"/> (una alerta operativa puede ser urgente
/// sin ser fraude, y una de auditoría puede ser de riesgo medio).
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
    /// Posible fraude o problema grave (gineteo, kiting, créditos no autorizados, cuadre
    /// forzado). Queda en la bandeja de auditoría del central; NO se envía al administrador
    /// de la estación.
    /// </summary>
    Auditoria = 2
}
