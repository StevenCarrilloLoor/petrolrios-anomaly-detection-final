namespace PetrolRios.Domain.Entities;

/// <summary>
/// Marca de que un usuario concreto ya vio una alerta. Es el estado leído/no leído POR USUARIO,
/// distinto del estado global de la alerta (Nueva/EnRevisión/…): si el administrador abre una alerta,
/// el auditor la sigue viendo como "nueva para él" hasta que él mismo la abra. Único por
/// (AlertaId, UsuarioId).
/// </summary>
public class AlertaVista : BaseEntity
{
    public int AlertaId { get; private set; }
    public int UsuarioId { get; private set; }
    public DateTime FechaVista { get; private set; } = DateTime.UtcNow;

    public static AlertaVista Create(int alertaId, int usuarioId) =>
        new()
        {
            AlertaId = alertaId,
            UsuarioId = usuarioId,
            FechaVista = DateTime.UtcNow
        };
}
