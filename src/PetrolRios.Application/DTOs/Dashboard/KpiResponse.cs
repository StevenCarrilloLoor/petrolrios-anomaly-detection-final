namespace PetrolRios.Application.DTOs.Dashboard;

public sealed record KpiResponse
{
    public int TotalAlertas { get; init; }
    public int AlertasNuevas { get; init; }
    public int AlertasCriticas { get; init; }
    public int AlertasEnRevision { get; init; }
    public int AlertasConfirmadas { get; init; }
    public int AlertasFalsoPositivo { get; init; }
    public double ScorePromedio { get; init; }

    /// <summary>Estaciones con agente conectado (ingesta en los últimos 10 minutos).</summary>
    public int EstacionesConectadas { get; init; }

    /// <summary>Total de estaciones registradas y activas en el sistema.</summary>
    public int EstacionesTotales { get; init; }
}

public sealed record AlertasPorTipoResponse(string TipoDetector, int Cantidad);

public sealed record AlertasPorEstacionResponse(int EstacionId, string EstacionNombre, int Cantidad);
