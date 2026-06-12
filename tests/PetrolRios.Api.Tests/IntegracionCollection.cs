namespace PetrolRios.Api.Tests;

/// <summary>
/// Colección compartida de tests de integración: un único Testcontainer de
/// PostgreSQL y un único host para todas las clases. Evita interferencias entre
/// fixtures (variables de entorno del connection string) y acelera la suite.
/// </summary>
[CollectionDefinition("Integracion")]
public sealed class IntegracionCollection : ICollectionFixture<PetrolRiosWebApplicationFactory>;
