namespace PetrolRios.Domain.Entities;

/// <summary>
/// Relación entre dos fuentes/tablas, para enriquecer las alertas de una regla personalizada con
/// campos de una tabla relacionada (estilo "lookup/linked fields"). Ejemplo: un Despacho
/// (<c>DetalleFactura</c>) se relaciona con su Factura (<c>Factura</c>) por el código de cliente,
/// de modo que una regla sobre despachos pueda mostrar la placa, el vendedor y el cliente —que
/// viven en la factura— en la evidencia de la alerta.
///
/// Se define UNA sola vez (auto-detectada al sembrar o creada por el Admin en lenguaje natural) y la
/// usan tanto el builder (ofrece los campos relacionados) como el detector (resuelve el cruce en
/// memoria, porque el ciclo ya tiene todas las tablas cargadas). Todo por datos: agregar una
/// relación nueva no toca el código del motor.
/// </summary>
public class RelacionTabla : BaseEntity
{
    /// <summary>Fuente desde la que parte la regla (p. ej. "DetalleFactura").</summary>
    public string FuenteOrigen { get; private set; } = string.Empty;

    /// <summary>Fuente relacionada cuyos campos se quieren traer (p. ej. "Factura").</summary>
    public string FuenteDestino { get; private set; } = string.Empty;

    /// <summary>Campo de la fuente origen que sirve de llave del cruce (p. ej. "CodigoCliente").</summary>
    public string CampoOrigen { get; private set; } = string.Empty;

    /// <summary>Campo de la fuente destino que debe coincidir (p. ej. "CodigoCliente").</summary>
    public string CampoDestino { get; private set; } = string.Empty;

    /// <summary>Etiqueta legible de la relación, en lenguaje natural (p. ej. "Factura del despacho").</summary>
    public string Etiqueta { get; private set; } = string.Empty;

    /// <summary>
    /// true si la relación la propuso el autodescubridor (cruzando nombres de llave + solapamiento de
    /// valores en staging), false si fue sembrada o creada a mano por el Admin. Sirve para distinguir
    /// las sugeridas de las confirmadas en la interfaz.
    /// </summary>
    public bool EsAutomatica { get; private set; }

    public bool Activa { get; set; } = true;

    public static RelacionTabla Create(
        string fuenteOrigen, string fuenteDestino, string campoOrigen, string campoDestino,
        string etiqueta, bool esAutomatica = false) =>
        new()
        {
            FuenteOrigen = fuenteOrigen.Trim(),
            FuenteDestino = fuenteDestino.Trim(),
            CampoOrigen = campoOrigen.Trim(),
            CampoDestino = campoDestino.Trim(),
            Etiqueta = string.IsNullOrWhiteSpace(etiqueta) ? $"{fuenteDestino} relacionada" : etiqueta.Trim(),
            EsAutomatica = esAutomatica,
            Activa = true
        };

    public void Actualizar(
        string fuenteOrigen, string fuenteDestino, string campoOrigen, string campoDestino,
        string etiqueta, bool activa)
    {
        FuenteOrigen = fuenteOrigen.Trim();
        FuenteDestino = fuenteDestino.Trim();
        CampoOrigen = campoOrigen.Trim();
        CampoDestino = campoDestino.Trim();
        Etiqueta = string.IsNullOrWhiteSpace(etiqueta) ? $"{fuenteDestino} relacionada" : etiqueta.Trim();
        Activa = activa;
        UpdatedAt = DateTime.UtcNow;
    }
}
