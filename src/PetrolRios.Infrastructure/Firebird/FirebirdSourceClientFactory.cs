using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Firebird;

/// <summary>
/// Crea instancias de FirebirdSourceClient para cada estación.
/// </summary>
public sealed class FirebirdSourceClientFactory : IFirebirdSourceClientFactory
{
    public IFirebirdSourceClient Create(string connectionString) =>
        new FirebirdSourceClient(connectionString);
}
