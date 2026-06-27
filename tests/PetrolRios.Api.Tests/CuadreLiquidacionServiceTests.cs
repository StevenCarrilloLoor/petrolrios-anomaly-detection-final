using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Infrastructure.Services;
using Xunit;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas de la lógica pura del cuadre de liquidación (mejora #3): dado un conjunto de cierres de
/// turno, liquidaciones y facturas, <see cref="CuadreLiquidacionService.CalcularTurnosSinLiquidar"/>
/// devuelve los turnos CERRADOS, con facturas FV, fuera del periodo de gracia y SIN fila en LIQU.
/// Son pruebas de lógica pura, sin base de datos.
/// </summary>
public class CuadreLiquidacionServiceTests
{
    private static readonly DateTime Ahora = new(2026, 6, 27, 12, 0, 0, DateTimeKind.Utc);
    private const double UmbralHoras = 12.0;

    // Cierre fuera del periodo de gracia (cerrado hace 24 h → ya debió liquidarse).
    private static CierreTurnoDto Cerrado(int turno, string estado = "1", double horasAtras = 24, string vend = "V1") =>
        new()
        {
            NumeroTurno = turno,
            EstadoTurno = estado,
            FechaFin = Ahora.AddHours(-horasAtras),
            CodigoVendedor = vend
        };

    private static LiquidacionDto Liquidacion(int turno) =>
        new() { NumeroLiquidacion = turno + 5000, NumeroTurno = turno, FechaLiquidacion = Ahora };

    private static FacturaDto Factura(int turno, double monto, string tipo = "FV", string num = "001-001-1") =>
        new() { NumeroTurno = turno, TotalNeto = monto, TipoDocumento = tipo, NumeroDocumento = num };

    [Fact]
    public void TurnoCerradoConFacturasNoLiquidado_SeReporta()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(101) },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(101, 30), Factura(101, 70) },
            UmbralHoras, Ahora);

        resultado.Should().ContainSingle();
        resultado[0].NumeroTurno.Should().Be(101);
        resultado[0].Facturas.Should().HaveCount(2);
        resultado[0].MontoTotal.Should().Be(100);
        resultado[0].Vendedor.Should().Be("V1");
    }

    [Fact]
    public void TurnoLiquidado_NoSeReporta()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(102) },
            new[] { Liquidacion(102) },
            new[] { Factura(102, 50) },
            UmbralHoras, Ahora);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public void TurnoAbierto_NoSeReporta()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(103, estado: "0") },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(103, 50) },
            UmbralHoras, Ahora);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public void TurnoCerradoDentroDelPeriodoDeGracia_NoSeReporta()
    {
        // Cerró hace 2 h (< 12 h de gracia): aún puede llegar su liquidación, no se alerta todavía.
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(104, horasAtras: 2) },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(104, 50) },
            UmbralHoras, Ahora);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public void TurnoCerradoSinFacturas_NoSeReporta()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(105) },
            Array.Empty<LiquidacionDto>(),
            Array.Empty<FacturaDto>(),
            UmbralHoras, Ahora);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public void TurnoCerradoSoloConDocumentosNoFV_NoSeReporta()
    {
        // Egresos (EB) y notas de crédito (DV) no son ventas: no cuentan para el cuadre.
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(106) },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(106, 50, tipo: "EB"), Factura(106, 20, tipo: "DV") },
            UmbralHoras, Ahora);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public void SoloCuentaFacturasFV_EnElMontoYConteo()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(107) },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(107, 40, tipo: "FV"), Factura(107, 999, tipo: "EB") },
            UmbralHoras, Ahora);

        resultado.Should().ContainSingle();
        resultado[0].Facturas.Should().HaveCount(1);
        resultado[0].MontoTotal.Should().Be(40);
    }

    [Fact]
    public void VariosTurnos_ReportaSoloLosQueIncumplen()
    {
        var cierres = new[]
        {
            Cerrado(201),                       // sin liquidar → reporta
            Cerrado(202),                       // liquidado    → no
            Cerrado(203, estado: "0"),          // abierto      → no
            Cerrado(204, horasAtras: 1),        // recién cerrado → no (gracia)
        };
        var liquidaciones = new[] { Liquidacion(202) };
        var facturas = new[]
        {
            Factura(201, 10), Factura(202, 10), Factura(203, 10), Factura(204, 10),
        };

        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            cierres, liquidaciones, facturas, UmbralHoras, Ahora);

        resultado.Should().ContainSingle();
        resultado[0].NumeroTurno.Should().Be(201);
    }

    [Fact]
    public void EstadoYTipoDocumentoToleranEspaciosYMayusculas()
    {
        var resultado = CuadreLiquidacionService.CalcularTurnosSinLiquidar(
            new[] { Cerrado(108, estado: " 1 ") },
            Array.Empty<LiquidacionDto>(),
            new[] { Factura(108, 50, tipo: " fv ") },
            UmbralHoras, Ahora);

        resultado.Should().ContainSingle();
        resultado[0].NumeroTurno.Should().Be(108);
    }
}
