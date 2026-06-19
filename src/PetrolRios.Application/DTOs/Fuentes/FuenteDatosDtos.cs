namespace PetrolRios.Application.DTOs.Fuentes;

/// <summary>Fuente de datos adicional del catálogo central (vista completa para administración).</summary>
public sealed record FuenteDatosResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Tabla { get; init; } = string.Empty;
    public string? ColumnaWatermark { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public bool Activa { get; init; }
}

/// <summary>Definición mínima que el agente necesita para extraer una fuente.</summary>
public sealed record FuenteDatosAgente(string Nombre, string Tabla, string? ColumnaWatermark);

public sealed record CrearFuenteDatosRequest(
    string Nombre, string Tabla, string? ColumnaWatermark, string? Descripcion);

public sealed record ActualizarFuenteDatosRequest(
    string Nombre, string Tabla, string? ColumnaWatermark, string? Descripcion, bool Activa);
