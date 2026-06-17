using FluentAssertions;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas de la máquina de estados del login por QR (estilo Steam).
/// </summary>
public class QrLoginServiceTests
{
    [Fact]
    public void Flujo_Completo_Iniciar_Aprobar_Consultar()
    {
        var sut = new QrLoginService();

        var (codigo, expira) = sut.Iniciar();
        codigo.Should().NotBeNullOrWhiteSpace();
        expira.Should().BeGreaterThan(0);

        // Pendiente hasta que alguien lo apruebe
        sut.Consultar(codigo).Estado.Should().Be(QrLoginService.EstadoQr.Pendiente);

        // Un usuario lo aprueba
        sut.Aprobar(codigo, usuarioId: 42).Should().BeTrue();

        var (estado, usuarioId) = sut.Consultar(codigo);
        estado.Should().Be(QrLoginService.EstadoQr.Aprobado);
        usuarioId.Should().Be(42);
    }

    [Fact]
    public void Aprobar_CodigoInexistente_Falla()
    {
        var sut = new QrLoginService();
        sut.Aprobar("codigo-que-no-existe", 1).Should().BeFalse();
    }

    [Fact]
    public void Consultar_CodigoInexistente_DevuelveNoExiste()
    {
        var sut = new QrLoginService();
        sut.Consultar("inventado").Estado.Should().Be(QrLoginService.EstadoQr.NoExiste);
    }

    [Fact]
    public void Consumir_ImpideReusoDelCodigo()
    {
        var sut = new QrLoginService();
        var (codigo, _) = sut.Iniciar();
        sut.Aprobar(codigo, 7);

        sut.Consumir(codigo);

        // Tras consumir, el código ya no sirve (un solo uso)
        sut.Consultar(codigo).Estado.Should().Be(QrLoginService.EstadoQr.NoExiste);
    }
}
