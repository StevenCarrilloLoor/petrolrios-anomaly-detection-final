using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors;
using PetrolRios.Detectors.Rules.InvoiceAnomaly;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Detector de precio fuera de lista contra el PRECIO OFICIAL regulado (tolerancia cero ± 1 centavo).
/// Mapeo confirmado: producto 1 = Súper (libre mercado, excluida), 2 = Extra/Ecopaís ($3,31), 3 = Diésel ($3,25).
/// </summary>
public sealed class PrecioFueraListaOficialTests
{
    private static readonly DateTime Desde = new(2026, 6, 12);
    private static readonly DateTime Hasta = new(2026, 7, 11);
    private readonly PrecioFueraListaRule _sut = new(new RiskScoringEngine());

    private static DetectionContext Contexto(params DetalleFacturaDto[] detalles) => new()
    {
        EstacionId = 1,
        EstacionNombre = "Test",
        FromWatermark = DateTime.UtcNow.AddDays(-1),
        ToWatermark = DateTime.UtcNow,
        Detalles = detalles,
        PreciosOficiales =
        [
            new PrecioOficialContexto("1", 5.65m, false, Desde, Hasta), // Súper: libre mercado
            new PrecioOficialContexto("2", 3.31m, true, Desde, Hasta),  // Extra/Ecopaís
            new PrecioOficialContexto("3", 3.25m, true, Desde, Hasta),  // Diésel
        ]
    };

    private static DetalleFacturaDto Det(string producto, double precio, DateTime? fecha = null) =>
        TestHelpers.CreateDetalle(
            producto: producto, valorUnitario: precio, cantidad: 5,
            fecha: fecha ?? new DateTime(2026, 6, 20, 10, 0, 0));

    [Fact]
    public void Regulado_AlPrecioOficial_NoMarca()
    {
        _sut.Evaluar(Contexto(Det("2", 3.31)), regla: null).Should().BeEmpty();
    }

    [Fact]
    public void Regulado_ConRedondeoDelPos_NoMarca()
    {
        // $3,312 = el surtidor cobra ~$0,002 sobre el oficial → dentro de la tolerancia.
        _sut.Evaluar(Contexto(Det("2", 3.312)), regla: null).Should().BeEmpty();
    }

    [Fact]
    public void Regulado_PorEncimaDelOficial_Marca()
    {
        var r = _sut.Evaluar(Contexto(Det("3", 3.40)), regla: null).ToList();
        r.Should().ContainSingle();
        r[0].Descripcion.Should().Contain("regulado").And.Contain("por encima");
    }

    [Fact]
    public void Regulado_PorDebajoDelOficial_Marca()
    {
        var r = _sut.Evaluar(Contexto(Det("2", 3.00)), regla: null).ToList();
        r.Should().ContainSingle();
        r[0].Descripcion.Should().Contain("por debajo");
    }

    [Fact]
    public void Super_AunqueElPrecioVarie_NoMarca()
    {
        // Súper es libre mercado: $6,50 ≠ $5,65 referencial, pero NO se marca (excluida).
        _sut.Evaluar(Contexto(Det("1", 6.50)), regla: null).Should().BeEmpty();
    }

    [Fact]
    public void SinPrecioOficialParaLaFecha_CaeAlMinimoDelDia()
    {
        // Fecha fuera de la vigencia oficial (mayo) → respaldo: mínimo del día; el caro (3,50 vs 3,10) se marca.
        var mayo = new DateTime(2026, 5, 20, 9, 0, 0);
        var r = _sut.Evaluar(Contexto(Det("2", 3.10, mayo), Det("2", 3.50, mayo)), regla: null).ToList();
        r.Should().ContainSingle();
        r[0].Descripcion.Should().Contain("autorizado"); // heurística, no "regulado"
    }
}
