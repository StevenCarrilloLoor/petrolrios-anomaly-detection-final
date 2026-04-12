using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Motor de scoring de riesgo: calcula score 0–100 con RiesgoBase × Multiplicadores.
/// Multiplicadores: por monto, reincidencia del empleado, historial de la estación.
/// </summary>
public sealed class RiskScoringEngine
{
    /// <summary>
    /// Calcula el score y nivel de riesgo.
    /// </summary>
    /// <param name="riesgoBase">Riesgo base del tipo de anomalía (0–100).</param>
    /// <param name="montoInvolucrado">Monto en dólares involucrado en la anomalía.</param>
    /// <param name="reincidenciasEmpleado">Cantidad de alertas previas del mismo empleado.</param>
    /// <param name="alertasHistoricasEstacion">Cantidad de alertas previas de la estación.</param>
    public (double Score, NivelRiesgo Nivel) Calculate(
        double riesgoBase,
        double montoInvolucrado = 0,
        int reincidenciasEmpleado = 0,
        int alertasHistoricasEstacion = 0)
    {
        var multiplicadorMonto = CalculateMultiplicadorMonto(montoInvolucrado);
        var multiplicadorReincidencia = CalculateMultiplicadorReincidencia(reincidenciasEmpleado);
        var multiplicadorEstacion = CalculateMultiplicadorEstacion(alertasHistoricasEstacion);

        var score = riesgoBase * multiplicadorMonto * multiplicadorReincidencia * multiplicadorEstacion;
        score = Math.Clamp(Math.Round(score, 2), 0, 100);

        var nivel = score switch
        {
            <= 25 => NivelRiesgo.Bajo,
            <= 50 => NivelRiesgo.Medio,
            <= 75 => NivelRiesgo.Alto,
            _ => NivelRiesgo.Critico
        };

        return (score, nivel);
    }

    // Monto: a mayor monto, mayor multiplicador (1.0 a 1.5)
    private static double CalculateMultiplicadorMonto(double monto) => monto switch
    {
        <= 0 => 1.0,
        <= 50 => 1.0,
        <= 200 => 1.1,
        <= 500 => 1.2,
        <= 1000 => 1.3,
        _ => 1.5
    };

    // Reincidencia del empleado: más alertas previas → mayor multiplicador (1.0 a 1.6)
    private static double CalculateMultiplicadorReincidencia(int reincidencias) => reincidencias switch
    {
        0 => 1.0,
        1 => 1.1,
        2 => 1.2,
        3 => 1.3,
        <= 5 => 1.4,
        _ => 1.6
    };

    // Historial de estación: estaciones con muchas alertas → mayor multiplicador (1.0 a 1.3)
    private static double CalculateMultiplicadorEstacion(int alertas) => alertas switch
    {
        <= 5 => 1.0,
        <= 15 => 1.1,
        <= 30 => 1.2,
        _ => 1.3
    };
}
