using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetrolRios.Application.DTOs.Combustible;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Services.Precios;

/// <summary>
/// Proveedor de precios por CASCADA de fuentes públicas oficiales (en orden de menor a mayor protección).
/// Es robusto y respetuoso: un solo User-Agent de navegador real, ETag/304 (no re-descarga si no cambió),
/// backoff (degrada una fuente 2 h ante 403/429), pausa entre fuentes y caída a la siguiente; NO hace
/// evasión de detección (no rota identidad ni falsifica el Referer). A ~1 request/día de datos públicos no
/// hace falta, y si una fuente bloquea hasta a un cliente educado, simplemente se usa la siguiente o el
/// precio del sistema. Devuelve el primer conjunto válido encontrado.
/// </summary>
public sealed class CascadaPreciosProvider : IProveedorPreciosExterno
{
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
    private static readonly TimeSpan Degradacion = TimeSpan.FromHours(2);

    private readonly HttpClient _http;
    private readonly ILogger<CascadaPreciosProvider> _logger;
    private readonly IReadOnlyList<(string Nombre, string Url)> _fuentes;

    // Estado en memoria por fuente (el central es un proceso de larga vida): ETag para 304 y hasta-cuándo
    // está degradada por un bloqueo previo. Se pierde al reiniciar (se re-descarga una vez), sin problema.
    private readonly ConcurrentDictionary<string, string> _etags = new();
    private readonly ConcurrentDictionary<string, DateTime> _degradadasHasta = new();
    private volatile string? _ultimaFuente;

    public CascadaPreciosProvider(
        HttpClient http,
        IOptions<PreciosCombustibleOptions> opts,
        ILogger<CascadaPreciosProvider> logger)
    {
        _http = http;
        _logger = logger;
        _fuentes = opts.Value.Fuentes is { Count: > 0 }
            ? opts.Value.Fuentes.Select(f => (f.Nombre, f.Url)).ToList()
            : PreciosCombustibleOptions.FuentesPorDefecto;
    }

    public bool Habilitado => _fuentes.Count > 0;
    public string? UltimaFuente => _ultimaFuente;
    public IReadOnlyList<string> FuentesDegradadas =>
        _degradadasHasta.Where(kv => kv.Value > DateTime.UtcNow).Select(kv => kv.Key).ToList();

    public async Task<IReadOnlyList<PrecioCombustibleExterno>?> ObtenerAsync(CancellationToken ct = default)
    {
        var (desde, hasta) = VigenciaActual();
        var primero = true;

        foreach (var (nombre, url) in _fuentes)
        {
            if (_degradadasHasta.TryGetValue(nombre, out var hastaCuando) && hastaCuando > DateTime.UtcNow)
                continue; // fuente degradada por un bloqueo previo: no malgastar requests

            if (!primero) await PausaCortesAsync(ct); // pausa entre fuentes de la cascada
            primero = false;

            try
            {
                var html = await DescargarAsync(nombre, url, ct);
                if (html is null) continue; // 304 (no cambió) o vacío → siguiente

                var precios = ParserPreciosHtml.Parsear(html);
                // Una fuente vale si trae al menos los regulados (Extra/Ecopaís/Diésel).
                var regulados = precios.Keys.Count(k => k.EsRegulado());
                if (regulados < 2) continue;

                _ultimaFuente = nombre;
                return precios
                    .Select(kv => new PrecioCombustibleExterno(
                        kv.Key.ToString(), kv.Value, 0m, desde, hasta))
                    .ToList();
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            {
                _degradadasHasta[nombre] = DateTime.UtcNow.Add(Degradacion);
                _logger.LogWarning("Fuente de precios '{Fuente}' degradada 2h ({Codigo}).", nombre, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fuente de precios '{Fuente}' falló; se intenta la siguiente.", nombre);
            }
        }

        _logger.LogWarning("Ninguna fuente de precios devolvió datos válidos; se conservan los del sistema.");
        return null;
    }

    /// <summary>Descarga el HTML con headers de navegador y ETag (304 → null = sin cambios).</summary>
    private async Task<string?> DescargarAsync(string nombre, string url, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
        req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        req.Headers.TryAddWithoutValidation("Accept-Language", "es-EC,es;q=0.9");
        if (_etags.TryGetValue(nombre, out var etag))
            req.Headers.TryAddWithoutValidation("If-None-Match", etag);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(12));
        using var resp = await _http.SendAsync(req, cts.Token);

        if (resp.StatusCode == HttpStatusCode.NotModified) return null; // 304: el precio no cambió
        resp.EnsureSuccessStatusCode();

        if (resp.Headers.ETag is { } nuevoEtag)
            _etags[nombre] = nuevoEtag.Tag;

        return await resp.Content.ReadAsStringAsync(cts.Token);
    }

    private static async Task PausaCortesAsync(CancellationToken ct)
    {
        // Pausa aleatoria 3–12 s entre fuentes (cortesía con los servidores).
        var segundos = Random.Shared.Next(3, 13);
        try { await Task.Delay(TimeSpan.FromSeconds(segundos), ct); } catch (TaskCanceledException) { }
    }

    /// <summary>Vigencia del período actual: del 12 del mes (o del mes anterior) al 11 del mes siguiente.</summary>
    private static (DateTime Desde, DateTime Hasta) VigenciaActual()
    {
        var hoy = DateTime.UtcNow.Date;
        var desde = hoy.Day >= 12
            ? new DateTime(hoy.Year, hoy.Month, 12)
            : new DateTime(hoy.AddMonths(-1).Year, hoy.AddMonths(-1).Month, 12);
        var hasta = desde.AddMonths(1).AddDays(-1);
        return (DateTime.SpecifyKind(desde, DateTimeKind.Utc), DateTime.SpecifyKind(hasta, DateTimeKind.Utc));
    }
}
