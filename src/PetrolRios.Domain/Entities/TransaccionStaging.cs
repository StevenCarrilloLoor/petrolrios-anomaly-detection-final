using System.Security.Cryptography;
using System.Text;

namespace PetrolRios.Domain.Entities;

public class TransaccionStaging : BaseEntity
{
    public int EstacionId { get; private set; }
    public string TipoTransaccion { get; private set; } = string.Empty;
    public string DataJson { get; private set; } = string.Empty;
    public DateTime FechaOriginal { get; private set; }
    public bool Procesada { get; set; }

    /// <summary>
    /// Huella SHA-256 del contenido (estación + tipo + datos). Permite descartar
    /// reenvíos idénticos del agente sin volver a insertarlos ni re-generar alertas
    /// (idempotencia de ingesta). Si el registro de origen cambia, su hash cambia y
    /// se trata como una transacción nueva (cosa deseable: detectar modificaciones).
    /// </summary>
    public string HashContenido { get; private set; } = string.Empty;

    public static TransaccionStaging Create(int estacionId, string tipoTransaccion, string dataJson, DateTime fechaOriginal) =>
        new()
        {
            EstacionId = estacionId,
            TipoTransaccion = tipoTransaccion,
            DataJson = dataJson,
            FechaOriginal = fechaOriginal,
            HashContenido = CalcularHash(estacionId, tipoTransaccion, dataJson)
        };

    /// <summary>Calcula la huella estable de una transacción para deduplicar.</summary>
    public static string CalcularHash(int estacionId, string tipoTransaccion, string dataJson)
    {
        var crudo = $"{estacionId}|{tipoTransaccion}|{dataJson}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(crudo));
        return Convert.ToHexString(bytes);
    }
}
