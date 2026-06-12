namespace PetrolRios.Domain.Enums;

public enum TipoDetector
{
    CashFraud = 1,
    InvoiceAnomaly = 2,
    PaymentFraud = 3,
    ComplianceViolation = 4,

    /// <summary>Reglas de negocio definidas por el usuario desde la interfaz.</summary>
    Personalizada = 5
}
