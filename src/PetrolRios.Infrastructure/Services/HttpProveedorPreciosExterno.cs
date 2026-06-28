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

    /// <summary>URL de una fuente externa que devuelva un JSON array de precios (alternativa simple; ver
    /// <see cref="HttpProveedorPreciosExterno"/>). Por defecto se usa la CASCADA de fuentes públicas.</summary>
    public string? FuenteUrl { get; set; }

    /// <summary>Cascada de fuentes a scrapear, en orden de preferencia. Vacío = se usan las de por defecto.</summary>
    public List<FuentePrecioConfig> Fuentes { get; set; } = [];

    /// <summary>Fuentes por defecto, de menor a mayor protección anti-bot (gobierno/gremio primero). Solo
    /// gasolinaecuador.com está verificada como HTML estático parseable; las demás son mejor-esfuerzo y la
    /// cascada cae a la siguiente si no responden o no traen precios. Configurables en appsettings.</summary>
    public static readonly IReadOnlyList<(string Nombre, string Url)> FuentesPorDefecto = new[]
    {
        ("arch", "https://www.arch.gob.ec/"),
        ("camddepe", "https://camddepe.com.ec/"),
        ("gasolinaecuador", "https://gasolinaecuador.com/"),
        ("primicias", "https://www.primicias.ec/economia/"),
    };
}

/// <summary>Una fuente configurable de la cascada (nombre corto + URL a scrapear).</summary>
public sealed class FuentePrecioConfig
{
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
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
