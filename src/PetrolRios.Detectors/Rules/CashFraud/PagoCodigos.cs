namespace PetrolRios.Detectors.Rules.CashFraud;

/// <summary>Códigos de pago de Contaplus compartidos por las reglas de efectivo/crédito.</summary>
internal static class PagoCodigos
{
    private static readonly string[] Efectivo = ["EF", "EFE"];
    private static readonly string[] Credito = ["CR", "CRE", "CRD"];

    public static bool EsEfectivo(string codigoPago) =>
        Efectivo.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool EsCredito(string codigoPago) =>
        Credito.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);
}
