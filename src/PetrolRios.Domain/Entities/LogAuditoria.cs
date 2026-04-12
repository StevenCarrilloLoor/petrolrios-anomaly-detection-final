namespace PetrolRios.Domain.Entities;

public class LogAuditoria : BaseEntity
{
    public string Accion { get; private set; } = string.Empty;
    public string Entidad { get; private set; } = string.Empty;
    public int? EntidadId { get; private set; }
    public string? DetalleJson { get; private set; }
    public string DireccionIp { get; private set; } = string.Empty;

    public int? UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }

    public static LogAuditoria Create(
        string accion,
        string entidad,
        int? entidadId = null,
        string? detalleJson = null,
        string direccionIp = "",
        int? usuarioId = null) =>
        new()
        {
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            DetalleJson = detalleJson,
            DireccionIp = direccionIp,
            UsuarioId = usuarioId
        };
}
