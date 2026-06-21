using PetrolRios.Application.DTOs.ReglasPersonalizadas;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Ejecuta el "backtest" (vista previa) de una regla personalizada borrador: la corre contra los
/// datos reales de staging de los últimos N días <b>sin guardarla ni generar alertas</b>, usando el
/// mismo motor que el ciclo de detección, para estimar cuántas alertas habría producido.
/// </summary>
public interface IReglaBacktestService
{
    Task<BacktestReglaResponse> EjecutarAsync(BacktestReglaRequest request, CancellationToken ct = default);
}
