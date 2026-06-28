using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services.Precios;

/// <summary>
/// Job de Hangfire que decide, en cada "tick" horario, si toca refrescar los precios y con qué cadencia.
/// MODO NORMAL (días 1–10): una verificación dentro de la hora 08:00 con jitter de 0–25 min, una sola vez
/// al día. MODO ALERTA (14:00 del 11 → 14:00 del 12): cada hora con jitter de 0–8 min, IDEMPOTENTE (si ya
/// capturó un precio nuevo en la ventana, los disparos siguientes no hacen nada). El jitter (dormir un rato
/// aleatorio antes del request) evita llegar siempre a la misma hora. La decisión de horario es pura y está
/// en <see cref="PlanificadorPrecios"/> (testeable); aquí va la idempotencia (contra la bitácora) y el jitter.
/// </summary>
public sealed class PreciosCombustibleScheduler
{
    private readonly IPreciosCombustibleService _precios;
    private readonly PetrolRiosDbContext _db;
    private readonly ILogger<PreciosCombustibleScheduler> _logger;

    public PreciosCombustibleScheduler(
        IPreciosCombustibleService precios,
        PetrolRiosDbContext db,
        ILogger<PreciosCombustibleScheduler> logger)
    {
        _precios = precios;
        _db = db;
        _logger = logger;
    }

    public async Task TickAsync(CancellationToken ct = default)
    {
        var ahoraEc = PlanificadorPrecios.AhoraEcuador(DateTime.UtcNow);
        var modo = PlanificadorPrecios.Modo(ahoraEc);

        switch (modo)
        {
            case ModoScrapePrecios.Alerta:
                if (await CapturoNuevoEnVentanaAsync(ct)) return; // idempotente: ya se obtuvo el precio nuevo
                await DormirJitterAsync(0, 8, ct);
                await _precios.RefrescarDesdeFuenteAsync("modo_alerta_horario", ct);
                break;

            case ModoScrapePrecios.Normal:
                var inicioHoyUtc = TimeZoneInfo.ConvertTimeToUtc(ahoraEc.Date, PlanificadorPrecios.ZonaEcuador);
                if (await YaCorrioDesdeAsync("modo_normal_08h", inicioHoyUtc, ct)) return; // 1 vez al día
                await DormirJitterAsync(0, 25, ct);
                await _precios.RefrescarDesdeFuenteAsync("modo_normal_08h", ct);
                break;

            // Inactivo: fuera de la hora normal y de la ventana de alerta → no hacer nada.
        }
    }

    /// <summary>Idempotencia de la ventana de alerta: ¿ya hubo una captura de precio NUEVO en las últimas ~30 h?</summary>
    private async Task<bool> CapturoNuevoEnVentanaAsync(CancellationToken ct)
    {
        var desde = DateTime.UtcNow.AddHours(-30);
        return await _db.PreciosCombustibleLog.AsNoTracking().AnyAsync(l =>
            l.Disparo == "modo_alerta_horario" && l.Resultado == "actualizado" && l.CreatedAt >= desde, ct);
    }

    /// <summary>¿Ya corrió ese disparo desde el instante indicado (p. ej. el inicio de hoy)?</summary>
    private async Task<bool> YaCorrioDesdeAsync(string disparo, DateTime desdeUtc, CancellationToken ct) =>
        await _db.PreciosCombustibleLog.AsNoTracking()
            .AnyAsync(l => l.Disparo == disparo && l.CreatedAt >= desdeUtc, ct);

    private async Task DormirJitterAsync(int minMinutos, int maxMinutos, CancellationToken ct)
    {
        var minutos = Random.Shared.Next(minMinutos, maxMinutos + 1);
        if (minutos <= 0) return;
        _logger.LogInformation("Precios: jitter de {Minutos} min antes del request.", minutos);
        try { await Task.Delay(TimeSpan.FromMinutes(minutos), ct); } catch (TaskCanceledException) { }
    }
}
