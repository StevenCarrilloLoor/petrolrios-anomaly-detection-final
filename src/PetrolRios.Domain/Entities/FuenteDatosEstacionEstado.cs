namespace PetrolRios.Domain.Entities;

/// <summary>
/// Último estado que una estación reportó para una fuente central. Permite distinguir
/// una tabla documentada de una tabla realmente validada, leída y enviada por el agente.
/// </summary>
public class FuenteDatosEstacionEstado : BaseEntity
{
    public int FuenteDatosId { get; private set; }
    public int EstacionId { get; private set; }
    public string Estado { get; private set; } = string.Empty;
    public bool TablaExiste { get; private set; }
    public bool ColumnaWatermarkValida { get; private set; }
    public int FilasLeidas { get; private set; }
    public int FilasEnviadas { get; private set; }
    public long TotalFilasEnviadas { get; private set; }
    public string? UltimoError { get; private set; }
    public DateTime VersionFuente { get; private set; }
    public DateTime UltimoReporte { get; private set; }
    public DateTime? UltimoExito { get; private set; }

    public FuenteDatos FuenteDatos { get; private set; } = null!;
    public Estacion Estacion { get; private set; } = null!;

    public static FuenteDatosEstacionEstado Create(
        int fuenteDatosId,
        int estacionId,
        string estado,
        bool tablaExiste,
        bool columnaWatermarkValida,
        int filasLeidas,
        int filasEnviadas,
        string? ultimoError,
        DateTime versionFuente)
    {
        var entidad = new FuenteDatosEstacionEstado
        {
            FuenteDatosId = fuenteDatosId,
            EstacionId = estacionId
        };
        entidad.Actualizar(
            estado, tablaExiste, columnaWatermarkValida, filasLeidas,
            filasEnviadas, ultimoError, versionFuente);
        return entidad;
    }

    public void Actualizar(
        string estado,
        bool tablaExiste,
        bool columnaWatermarkValida,
        int filasLeidas,
        int filasEnviadas,
        string? ultimoError,
        DateTime versionFuente)
    {
        Estado = estado.Trim();
        TablaExiste = tablaExiste;
        ColumnaWatermarkValida = columnaWatermarkValida;
        FilasLeidas = Math.Max(0, filasLeidas);
        FilasEnviadas = Math.Max(0, filasEnviadas);
        TotalFilasEnviadas += FilasEnviadas;
        UltimoError = string.IsNullOrWhiteSpace(ultimoError) ? null : ultimoError.Trim();
        VersionFuente = versionFuente.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(versionFuente, DateTimeKind.Utc)
            : versionFuente.ToUniversalTime();
        UltimoReporte = DateTime.UtcNow;
        if (TablaExiste
            && ColumnaWatermarkValida
            && UltimoError is null
            && Estado is "Sincronizada" or "SinDatos" or "DatosLeidos")
            UltimoExito = UltimoReporte;
    }
}
