namespace PetrolRios.Application.DTOs.Combustible;

/// <summary>Precio vigente de un combustible, con el valor del SISTEMA (efectivo) y el último observado
/// por la API/scraper, para verlos lado a lado en el dashboard.</summary>
public sealed record PrecioCombustibleResponse(
    string Producto,
    string Nombre,
    bool EsRegulado,
    decimal PrecioGalon,          // precio del SISTEMA (efectivo: el que sirve y usan los detectores)
    decimal? PrecioApi,           // último precio observado por el scraper (null si aún no hay)
    string? FuenteApi,
    DateTime? ApiActualizadoEn,
    decimal Subsidio,
    bool PrecioPendiente,
    DateTime VigenteDesde,
    DateTime? VigenteHasta,
    string Fuente);

/// <summary>Respuesta de precios vigentes con metadatos (moneda, momento, nota de bandas, fuentes degradadas).</summary>
public sealed record PreciosCombustibleResponse(
    IReadOnlyList<PrecioCombustibleResponse> Precios,
    string Moneda,
    DateTime ConsultadoEn,
    string Nota,
    IReadOnlyList<string> FuentesDegradadas);

/// <summary>Cuerpo para actualizar (admin) el precio de un combustible cuando cambia la banda mensual.</summary>
public sealed record ActualizarPrecioCombustibleRequest(
    string Producto,
    decimal PrecioGalon,
    decimal Subsidio,
    DateTime VigenteDesde,
    DateTime? VigenteHasta,
    string? Fuente);

/// <summary>Precio entregado por una fuente externa (conector). Producto en texto: Extra/Ecopais/Diesel/Super.</summary>
public sealed record PrecioCombustibleExterno(
    string Producto,
    decimal PrecioGalon,
    decimal Subsidio,
    DateTime VigenteDesde,
    DateTime? VigenteHasta);

/// <summary>Salud del subsistema de precios: modo del schedule, estado, última actualización y fuentes caídas.</summary>
public sealed record SaludPreciosResponse(
    string ModoSchedule,                       // Normal | Alerta | Inactivo
    string Estado,                             // OK | Warning | Error | Critico | Urgente
    string? Detalle,
    DateTime? UltimaActualizacion,
    string? UltimaFuente,
    string? UltimoError,
    IReadOnlyList<string> FuentesDegradadas);

/// <summary>Una entrada de la bitácora de precios (para el historial).</summary>
public sealed record HistorialPrecioItem(
    DateTime Fecha,
    string Producto,
    decimal? PrecioAnterior,
    decimal? PrecioNuevo,
    decimal? VariacionPorcentual,
    string Fuente,
    string Disparo,
    string Resultado);
