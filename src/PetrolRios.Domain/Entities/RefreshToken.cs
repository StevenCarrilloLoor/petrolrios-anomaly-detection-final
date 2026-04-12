namespace PetrolRios.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool Revoked { get; set; }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !Revoked && !IsExpired;

    public static RefreshToken Create(string token, DateTime expiresAt, int usuarioId) =>
        new() { Token = token, ExpiresAt = expiresAt, UsuarioId = usuarioId };
}
