namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Factoría que crea un IFirebirdSourceClient por connection string de estación.
/// </summary>
public interface IFirebirdSourceClientFactory
{
    IFirebirdSourceClient Create(string connectionString);
}
