using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules.InvoiceAnomaly;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Helpers para construir datos sintéticos en tests de detectores.
/// </summary>
internal static class TestHelpers
{
    public static DetectionContext CreateContext(
        int estacionId = 1,
        string estacionNombre = "Estacion Test",
        IReadOnlyList<FacturaDto>? facturas = null,
        IReadOnlyList<DetalleFacturaDto>? detalles = null,
        IReadOnlyList<CierreTurnoDto>? cierresTurno = null,
        IReadOnlyList<DepositoTurnoDto>? depositosTurno = null,
        IReadOnlyList<AnulacionDto>? anulaciones = null,
        IReadOnlyList<CreditoDto>? creditos = null,
        IReadOnlyList<TarjetaTurnoDto>? tarjetasTurno = null,
        IReadOnlyList<ReglaDeteccion>? reglas = null,
        IReadOnlyList<ReglaPersonalizada>? reglasPersonalizadas = null,
        IReadOnlyDictionary<string, int>? alertasPrevias = null,
        IReadOnlyDictionary<string, IReadOnlyList<IDictionary<string, object>>>? fuentesGenericas = null,
        TimeOnly? horaApertura = null,
        TimeOnly? horaCierre = null) => new()
        {
            FuentesGenericas = fuentesGenericas ?? new Dictionary<string, IReadOnlyList<IDictionary<string, object>>>(),
            EstacionId = estacionId,
            EstacionNombre = estacionNombre,
            FromWatermark = DateTime.UtcNow.AddHours(-1),
            ToWatermark = DateTime.UtcNow,
            Facturas = facturas ?? [],
            Detalles = detalles ?? [],
            CierresTurno = cierresTurno ?? [],
            DepositosTurno = depositosTurno ?? [],
            Anulaciones = anulaciones ?? [],
            Creditos = creditos ?? [],
            TarjetasTurno = tarjetasTurno ?? [],
            Reglas = reglas ?? DefaultReglas(),
            ReglasPersonalizadas = reglasPersonalizadas ?? [],
            AlertasPreviasPorEmpleado = alertasPrevias ?? new Dictionary<string, int>(),
            HoraApertura = horaApertura ?? new TimeOnly(6, 0),
            HoraCierre = horaCierre ?? new TimeOnly(22, 0)
        };

    public static IReadOnlyList<ReglaDeteccion> DefaultReglas() =>
    [
        // Cash Fraud
        ReglaDeteccion.Create(TipoDetector.CashFraud, "R1", "D1", "DiferenciaEfectivoUmbral", 50.0),
        ReglaDeteccion.Create(TipoDetector.CashFraud, "R2", "D2", "FaltantesRecurrentesMaximo", 3.0),
        ReglaDeteccion.Create(TipoDetector.CashFraud, "R3", "D3", "FaltantesRecurrentesDias", 30.0),
        ReglaDeteccion.Create(TipoDetector.CashFraud, "R3a", "D3a", "CreditoSinClienteHabilitado", 1.0),
        ReglaDeteccion.Create(TipoDetector.CashFraud, "R3b", "D3b", "EfectivoCorporativoPorcentajeUmbral", 30.0),
        // Invoice Anomaly
        ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "R4", "D4", "AnulacionesPorcentajeUmbral", 5.0),
        ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "R5", "D5", "PrecioFueraListaHabilitado", 1.0),
        ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "R6", "D6", "CamposObligatoriosHabilitado", 1.0, AmbitoAlerta.Operativa),
        ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "R6a", "D6a", "DescuentoPorcentajeMaximo", 10.0),
        ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "R6b", "D6b", "TotalInconsistenteHabilitado", 1.0),
        // Payment Fraud
        ReglaDeteccion.Create(TipoDetector.PaymentFraud, "R7", "D7", "ReversionTarjetaMinutosUmbral", 30.0),
        ReglaDeteccion.Create(TipoDetector.PaymentFraud, "R8", "D8", "CreditoSinAutorizacionHabilitado", 1.0),
        ReglaDeteccion.Create(TipoDetector.PaymentFraud, "R9", "D9", "DuplicadaMinutosUmbral", 5.0),
        ReglaDeteccion.Create(TipoDetector.PaymentFraud, "R9a", "D9a", "DespachosRapidosMinutosUmbral", 10.0),
        // Compliance Violation
        ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "R10", "D10", "PlacaGenericaGalonesMaximo", 5.0),
        ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "R11", "D11", "MultipleCombustibleHabilitado", 1.0),
        ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "R11a", "D11a", "VentaSinPlacaMontoMinimo", 200.0),
        ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "R12", "D12", "FueraHorarioHabilitado", 1.0)
    ];

    /// <summary>
    /// Construye el InvoiceAnomalyDetector con sus reglas Strategy (las mismas que registra la DI).
    /// El detector ya no contiene la lógica: orquesta estas reglas.
    /// </summary>
    public static InvoiceAnomalyDetector CrearInvoiceAnomalyDetector(RiskScoringEngine scoring) =>
        new(
            new IDetectionRule[]
            {
                new TasaAnulacionesRule(scoring),
                new PrecioFueraListaRule(scoring),
                new CamposObligatoriosRule(scoring),
                new DescuentoExcesivoRule(scoring),
                new TotalInconsistenteRule(scoring),
                new FechaFueraDeRangoRule(scoring),
                new AnulacionRecurrenteRule(scoring),
                new DespachoNoFacturadoRule(scoring),
            },
            NullLogger<InvoiceAnomalyDetector>.Instance);

    /// <summary>Crea una regla desactivada para probar el respeto al flag Activa.</summary>
    public static ReglaDeteccion CreateReglaInactiva(
        TipoDetector tipo, string parametro, double umbral)
    {
        var regla = ReglaDeteccion.Create(tipo, $"Regla {parametro}", "Desactivada", parametro, umbral);
        regla.Activa = false;
        return regla;
    }

    public static FacturaDto CreateFactura(
        double secuencia = 1, string vendedor = "V001", string placa = "ABC1234",
        string ruc = "1234567890", int turno = 100, double totalNeto = 100,
        string codigoPago = "EF", DateTime? fecha = null, string manguera = "01",
        string codigoCliente = "C001", double descuento = 0,
        double? subtotal = null, double? iva = null) => new()
        {
            SecuenciaDocumento = secuencia,
            TipoDocumento = "FV",
            NumeroDocumento = $"001-001-{secuencia:0000000}",
            FechaDocumento = fecha ?? DateTime.UtcNow.AddMinutes(-30),
            CodigoCliente = codigoCliente,
            TotalNeto = totalNeto,
            TotalSinIva = totalNeto / 1.12,
            Descuento = descuento,
            Iva = iva ?? (totalNeto - (subtotal ?? totalNeto / 1.12) + descuento),
            CodigoVendedor = vendedor,
            CodigoPago = codigoPago,
            Placa = placa,
            RucCliente = ruc,
            NumeroTurno = turno,
            Subtotal = subtotal ?? totalNeto / 1.12,
            NumeroConsecutivo = (int)secuencia,
            CodigoChofer = "",
            CodigoManguera = manguera
        };

    public static CierreTurnoDto CreateCierreTurno(
        int turno = 100, string vendedor = "V001", double faltante = 0,
        double sobrante = 0, double ingresos = 1000,
        DateTime? fechaInicio = null, DateTime? fechaFin = null,
        string estadoTurno = "1") => new()
        {
            NumeroTurno = turno,
            CodigoVendedor = vendedor,
            FechaInicio = fechaInicio ?? DateTime.UtcNow.AddHours(-8),
            FechaFin = fechaFin ?? DateTime.UtcNow,
            EstadoTurno = estadoTurno,
            SaldoInicial = 100,
            Ingresos = ingresos,
            Egresos = 0,
            SaldoFinal = 100 + ingresos,
            Faltante = faltante,
            Sobrante = sobrante,
            Creditos = 0
        };

    public static DepositoTurnoDto CreateDeposito(
        int turno = 100, string vendedor = "V001", decimal total = 1000,
        string tipo = "EF") => new()
        {
            NumeroDeposito = 1,
            CodigoVendedor = vendedor,
            NumeroTurno = turno,
            FechaDeposito = DateTime.UtcNow.AddMinutes(-10),
            TipoDeposito = tipo,
            Detalle = "Depósito efectivo",
            Cantidad = 1,
            Valor = total,
            Total = total
        };

    public static DetalleFacturaDto CreateDetalle(
        double numero = 1, double cantidad = 10, double valorUnitario = 2.50,
        string producto = "01", string manguera = "01", DateTime? fecha = null,
        string facturado = "1") => new()
        {
            NumeroDespacho = numero,
            CodigoManguera = manguera,
            FechaDespacho = fecha ?? DateTime.UtcNow.AddMinutes(-20),
            VolumenTotal = cantidad * valorUnitario,
            Cantidad = cantidad,
            ValorUnitario = valorUnitario,
            CodigoProducto = producto,
            NombreProducto = $"Producto {producto}",
            CodigoCliente = "C001",
            Facturado = facturado
        };

    public static AnulacionDto CreateAnulacion(DateTime? fecha = null) => new()
    {
        NumeroAnulacion = 1,
        TipoComprobante = "FV",
        FechaAnulacion = fecha ?? DateTime.UtcNow.AddMinutes(-15),
        Establecimiento = "001",
        PuntoEmision = "001",
        SecuencialInicio = "0000001",
        SecuencialFin = "0000001",
        Autorizacion = "AUT001"
    };

    public static CreditoDto CreateCredito(
        double total = 500, double comprobante = 0, string socio = "S001",
        DateTime? fecha = null, string garante = "G001") => new()
        {
            NumeroCabecera = 1,
            FechaCabecera = fecha ?? DateTime.UtcNow.AddMinutes(-30),
            CodigoCredito = "01",
            CodigoSocio = socio,
            PlazoCabecera = 30,
            TasaCredito = 0.15,
            CodigoGarante = garante,
            TotalCredito = total,
            TotalInteres = total * 0.15,
            CodigoBanco = "01",
            NumeroComprobante = comprobante
        };

    public static TarjetaTurnoDto CreateTarjetaTurno(
        int id = 1, int turno = 100, string banco = "01",
        int cantidad = 1, decimal valor = 50) => new()
        {
            NumeroTarjetaTurno = id,
            NumeroTurno = turno,
            CodigoBanco = banco,
            Cantidad = cantidad,
            Valor = valor
        };
}
