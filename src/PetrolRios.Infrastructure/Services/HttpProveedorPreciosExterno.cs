using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetrolRios.Application.DTOs.Combustible;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Services;

/// <summary>Opciones del conector de precios. <c>FuenteUrl</c> vacío = sin fuente externa (se usan los
/// valores guardados/sembrados). Se configura en <c>appsettings</c> bajo la sección "PreciosCombustible".</summary>
public sealed class PreciosCombustibleOptions
{
    public const string Section = "PreciosCombustible";

    /// <summary>URL de una fuente externa que devuelva un JSON array de precios (ver
    /// <see cref="PrecioCombustibleExterno"/>). Vacío por defecto: no hay API pública oficial de Ecuador,
    /// así que el sistema sirve los valores oficiales guardados y este conector queda listo para cuando
    /// se disponga de una fuente (propia o de terceros).</summary>
    public string? FuenteUrl { get; set; }
}

/// <summary>
/// Conector HTTP a una fuente externa de precios. Si <c>FuenteUrl</c> está configurada, hace GET y espera
/// un JSON array de <see cref="PrecioCombustibleExterno"/>. Ante cualquier fallo devuelve null para que el
/// servicio caiga con elegancia a los valores guardados (robustez: nunca rompe por una fuente caída).
/// </summary>
public sealed class HttpProveedorPreciosExterno : IProveedorPreciosExterno
{
    private readonly HttpClient _http;
    private readonly string? _url;
    private readonly ILogger<HttpProveedorPreciosExterno> _logger;

    public HttpProveedorPreciosExterno(
        HttpClient http,
        IOptions<PreciosCombustibleOptions> opts,
        ILogger<HttpProveedorPreciosExterno> logger)
    {
        _http = http;
        _url = opts.Value.FuenteUrl?.Trim();
        _logger = logger;
    }

    public bool Habilitado => !string.IsNullOrWhiteSpace(_url);

    public async Task<IReadOnlyList<PrecioCombustibleExterno>?> ObtenerAsync(CancellationToken ct = default)
    {
        if (!Habilitado) return null;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            var json = await _http.GetStringAsync(_url!, cts.Token);
            return JsonSerializer.Deserialize<List<PrecioCombustibleExterno>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "La fuente externa de precios no respondió ({Url}).", _url);
            return null;
        }
    }
}
