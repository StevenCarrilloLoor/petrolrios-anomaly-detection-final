namespace PetrolRios.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public int RolId { get; private set; }
    public Rol Rol { get; private set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<AsignacionAlerta> Asignaciones { get; private set; } = [];
    public ICollection<LogAuditoria> Logs { get; private set; } = [];

    public static Usuario Create(string email, string nombreCompleto, string passwordHash, int rolId) =>
        new() { Email = email, NombreCompleto = nombreCompleto, PasswordHash = passwordHash, RolId = rolId };

    public void UpdatePassword(string newHash) => PasswordHash = newHash;
}
