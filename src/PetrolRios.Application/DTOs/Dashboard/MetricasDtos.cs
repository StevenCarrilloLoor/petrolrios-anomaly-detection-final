namespace PetrolRios.Application.DTOs.Dashboard;

/// <summary>Punto de la serie temporal de alertas por día (CU-13).</summary>
public sealed record TendenciaDiaResponse(DateTime Fecha, int Total, int Criticas, int Altas);

/// <summary>Conteo de alertas por nivel de riesgo.</summary>
public sealed record AlertasPorNivelResponse(string NivelRiesgo, int Cantidad);

/// <summary>Empleados con mayor número de alertas (ranking de riesgo).</summary>
public sealed record TopEmpleadoResponse(
    string EmpleadoCodigo,
    int CantidadAlertas,
    double ScorePromedio,
    int Criticas,
    string EstacionNombre,
    string? EmpleadoNombre = null);

/// <summary>Métricas de efectividad y resolución para el dashboard ejecutivo (CU-13).</summary>
public sealed record MetricasResolucionResponse
{
    /// <summary>Tiempo medio entre detección y resolución, en horas.</summary>
    public double TiempoMedioResolucionHoras { get; init; }

    /// <summary>Porcentaje de alertas resueltas marcadas como falso positivo.</summary>
    public double TasaFalsosPositivos { get; init; }

    /// <summary>Porcentaje de alertas resueltas confirmadas como irregularidad real
    /// (indicador de la tasa de alertas válidas del OE2, meta &gt; 90%).</summary>
    public double TasaAlertasValidas { get; init; }

    /// <summary>Alertas detectadas en las últimas 24 horas.</summary>
    public int AlertasUltimas24Horas { get; init; }

    /// <summary>Total de alertas resueltas (confirmadas + falsos positivos + cerradas).</summary>
    public int TotalResueltas { get; init; }

    /// <summary>Alertas aún pendientes de revisión.</summary>
    public int TotalPendientes { get; init; }
}
