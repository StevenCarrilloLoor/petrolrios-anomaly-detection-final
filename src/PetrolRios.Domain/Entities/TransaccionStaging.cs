namespace PetrolRios.Domain.Entities;

public class TransaccionStaging : BaseEntity
{
    public int EstacionId { get; private set; }
    public string TipoTransaccion { get; private set; } = string.Empty;
    public string DataJson { get; private set; } = string.Empty;
    public DateTime FechaOriginal { get; private set; }
    public bool Procesada { get; set; }

    public static TransaccionStaging Create(int estacionId, string tipoTransaccion, string dataJson, DateTime fechaOriginal) =>
        new()
        {
            EstacionId = estacionId,
            TipoTransaccion = tipoTransaccion,
            DataJson = dataJson,
            FechaOriginal = fechaOriginal
        };
}
