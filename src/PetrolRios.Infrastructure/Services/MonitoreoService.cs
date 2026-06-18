using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Monitoreo;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Hubs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Estado de las conexiones del aplicativo: agentes por estación (última ingesta
/// real en staging), base de datos, SignalR y último ciclo del motor de detección.
/// </summary>
public sealed class MonitoreoService : IMonitoreoService
{
    /// <summary>
    /// Un agente se considera en línea si envió un heartbeat hace menos de 3 minutos
    /// (el agente late en cada ciclo, aunque no haya transacciones nuevas).
    /// </summary>
    public static readonly TimeSpan VentanaConexion = TimeSpan.FromMinutes(3);

    private static readonly DateTime InicioProceso = Process.GetCurrentProcess().StartTime.ToUniversalTime();

    private readonly PetrolRiosDbContext _dbContext;

    public MonitoreoService(PetrolRiosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ConexionEstacionResponse>> GetConexionesEstacionesAsync(CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var hace24Horas = ahora.AddHours(-24);

        var estaciones = await _dbContext.Estaciones
            .OrderBy(e => e.Codigo)
            .Select(e => new { e.Id, e.Codigo, e.Nombre, e.Zona, e.Activa, e.UltimoHeartbeat, e.VersionAgente, e.HoraApertura, e.HoraCierre, e.CorreoContacto })
            .ToListAsync(ct);

        // Estadísticas de ingesta por estación (la última ingesta REAL, no el seed)
        var estadisticas = await _dbContext.TransaccionesStaging
            .GroupBy(s => s.EstacionId)
            .Select(g => new
            {
                EstacionId = g.Key,
                UltimaIngesta = g.Max(s => s.CreatedAt),
                Total = g.Count(),
                Ultimas24h = g.Count(s => s.CreatedAt >= hace24Horas),
                Pendientes = g.Count(s => !s.Procesada)
            })
            .ToDictionaryAsync(x => x.EstacionId, ct);

        return estaciones.Select(e =>
        {
            var stats = estadisticas.GetValueOrDefault(e.Id);

            var ultimaIngesta = stats?.UltimaIngesta;
            var minutosIngesta = ultimaIngesta.HasValue
                ? Math.Round((ahora - ultimaIngesta.Value).TotalMinutes, 1)
                : (double?)null;

            // El estado de conexión lo da el HEARTBEAT del agente, no los datos:
            // un agente activo sin transacciones nuevas sigue estando en línea.
            var minutosHeartbeat = e.UltimoHeartbeat.HasValue
                ? Math.Round((ahora - e.UltimoHeartbeat.Value).TotalMinutes, 1)
                : (double?)null;
            var conectada = e.UltimoHeartbeat.HasValue
                && (ahora - e.UltimoHeartbeat.Value) < VentanaConexion;

            return new ConexionEstacionResponse
            {
                EstacionId = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                Zona = e.Zona ?? string.Empty,
                Activa = e.Activa,
                HoraApertura = e.HoraApertura.ToString("HH:mm"),
                HoraCierre = e.HoraCierre.ToString("HH:mm"),
                CorreoContacto = e.CorreoContacto,
                Conectada = conectada,
                Estado = conectada
                    ? "En línea"
                    : e.UltimoHeartbeat.HasValue ? "Sin conexión" : "Nunca conectada",
                UltimoHeartbeat = e.UltimoHeartbeat,
                MinutosDesdeUltimoHeartbeat = minutosHeartbeat,
                VersionAgente = e.VersionAgente,
                UltimaIngesta = ultimaIngesta,
                MinutosDesdeUltimaIngesta = minutosIngesta,
                TransaccionesUltimas24Horas = stats?.Ultimas24h ?? 0,
                TransaccionesTotales = stats?.Total ?? 0,
                PendientesAnalisis = stats?.Pendientes ?? 0
            };
        }).ToList();
    }

    public async Task<EstadoSistemaResponse> GetEstadoSistemaAsync(CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;

        // Latencia de la base de datos (ping simple)
        var sw = Stopwatch.StartNew();
        bool bdOk;
        try
        {
            bdOk = await _dbContext.Database.CanConnectAsync(ct);
        }
        catch
        {
            bdOk = false;
        }
        sw.Stop();

        // Último ciclo del motor de detección
        var ultimoCiclo = await _dbContext.EjecucionesJob
            .OrderByDescending(j => j.Id)
            .Select(j => new { j.FechaInicio, j.Estado, j.AlertasGeneradas, j.DuracionSegundos })
            .FirstOrDefaultAsync(ct);

        // Estaciones en línea (heartbeat del agente dentro de la ventana)
        var limite = ahora - VentanaConexion;
        var estacionesConectadas = await _dbContext.Estaciones
            .CountAsync(e => e.Activa && e.UltimoHeartbeat >= limite, ct);

        return new EstadoSistemaResponse
        {
            VersionApi = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "2.0.0",
            InicioApi = InicioProceso,
            UptimeSegundos = Math.Round((ahora - InicioProceso).TotalSeconds),
            Entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            BaseDatosConectada = bdOk,
            LatenciaBaseDatosMs = bdOk ? Math.Round(sw.Elapsed.TotalMilliseconds, 1) : null,
            ClientesSignalRConectados = AlertsHub.ConexionesActivas,
            EstacionesConectadas = estacionesConectadas,
            EstacionesTotales = await _dbContext.Estaciones.CountAsync(e => e.Activa, ct),
            UltimoCicloDeteccion = ultimoCiclo?.FechaInicio,
            UltimoCicloEstado = ultimoCiclo == null
                ? null
                : ultimoCiclo.Estado == EstadoJob.Completado ? "Completado"
                : ultimoCiclo.Estado == EstadoJob.Fallido ? "Fallido"
                : ultimoCiclo.Estado.ToString(),
            UltimoCicloAlertas = ultimoCiclo?.AlertasGeneradas,
            UltimoCicloDuracionSegundos = ultimoCiclo?.DuracionSegundos,
            MinutosDesdeUltimoCiclo = ultimoCiclo is null
                ? null
                : Math.Round((ahora - ultimoCiclo.FechaInicio).TotalMinutes, 1)
        };
    }
}
