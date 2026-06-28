namespace PetrolRios.Application.DTOs.Combustible;

/// <summary>Precio vigente de un combustible regulado (para el dashboard y la API).</summary>
public sealed record PrecioCombustibleResponse(
    string Producto,
    string Nombre,
    decimal PrecioGalon,
    decimal Subsidio,
    DateTime VigenteDesde,
    DateTime? VigenteHasta,
    string Fuente);

/// <summary>Respuesta de precios vigentes con metadatos (moneda, momento de consulta, nota de bandas).</summary>
public sealed record PreciosCombustibleResponse(
    IReadOnlyList<PrecioCombustibleResponse> Precios,
    string Moneda,
    DateTime ConsultadoEn,
    string Nota);

/// <summary>Cuerpo para actualizar (admin) el precio de un combustible cuando cambia la banda mensual.</summary>
public sealed record ActualizarPrecioCombustibleRequest(
    string Producto,
    decimal PrecioGalon,
    decimal Subsidio,
    DateTime VigenteDesde,
    DateTime? VigenteHasta,
    string? Fuente);

/// <summary>Precio entregado por una fuente externa (conector). Producto en texto: Extra/Ecopais/Diesel.</summary>
public sealed record PrecioCombustibleExterno(
    string Producto,
    decimal PrecioGalon,
    decimal Subsidio,
    DateTime VigenteDesde,
    DateTime? VigenteHasta);
