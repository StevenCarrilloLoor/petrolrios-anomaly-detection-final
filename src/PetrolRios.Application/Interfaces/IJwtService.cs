using PetrolRios.Domain.Entities;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Servicio de generación y validación de tokens JWT.
/// </summary>
public interface IJwtService
{
    string GenerateToken(Usuario usuario, string rolNombre);
    string GenerateRefreshToken();
}
