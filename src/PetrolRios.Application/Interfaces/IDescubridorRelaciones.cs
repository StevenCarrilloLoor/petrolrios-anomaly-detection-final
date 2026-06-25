namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Autodescubridor de relaciones entre fuentes/tablas. Propone relaciones cruzando las llaves de
/// negocio compartidas (mismo concepto o mismo nombre de columna) y las valida con el solapamiento
/// de valores reales en staging, para que el creador de reglas ofrezca campos relacionados sin que
/// nadie tenga que definir las uniones a mano.
/// </summary>
public interface IDescubridorRelaciones
{
    /// <summary>
    /// Ejecuta el descubrimiento y persiste las relaciones nuevas (marcadas como automáticas).
    /// Devuelve cuántas se crearon. Idempotente: no duplica las que ya existen.
    /// </summary>
    Task<int> DescubrirAsync(CancellationToken ct);
}
