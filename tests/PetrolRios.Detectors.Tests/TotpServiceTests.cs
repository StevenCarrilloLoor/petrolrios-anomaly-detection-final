using FluentAssertions;
using PetrolRios.Application.Security;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Pruebas del autenticador TOTP (2FA, RFC 6238) compatible con Google Authenticator.
/// </summary>
public class TotpServiceTests
{
    private readonly TotpService _sut = new();

    [Fact]
    public void GenerarSecreto_DevuelveBase32Valido()
    {
        var secreto = _sut.GenerarSecreto();
        secreto.Should().NotBeNullOrWhiteSpace();
        secreto.Should().MatchRegex("^[A-Z2-7]+$"); // alfabeto Base32
        secreto.Length.Should().BeGreaterThanOrEqualTo(16);
    }

    [Fact]
    public void UriOtpauth_TieneFormatoEscaneablePorLaApp()
    {
        var secreto = _sut.GenerarSecreto();
        var uri = _sut.ConstruirUriOtpauth(secreto, "admin@petrolrios.com");

        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain($"secret={secreto}");
        uri.Should().Contain("issuer=PetrolRios");
        uri.Should().Contain("digits=6");
        uri.Should().Contain("period=30");
    }

    [Fact]
    public void CodigoActual_EsAceptadoPorValidar()
    {
        var secreto = _sut.GenerarSecreto();
        var codigo = _sut.GenerarCodigoActual(secreto);

        codigo.Should().HaveLength(6).And.MatchRegex("^[0-9]{6}$");
        _sut.Validar(secreto, codigo).Should().BeTrue();
    }

    [Fact]
    public void Validar_RechazaCodigosInvalidos()
    {
        var secreto = _sut.GenerarSecreto();
        _sut.Validar(secreto, "000000").Should().BeFalse();
        _sut.Validar(secreto, "").Should().BeFalse();
        _sut.Validar(secreto, "123").Should().BeFalse();
        _sut.Validar(secreto, "abcdef").Should().BeFalse();
    }

    [Fact]
    public void Validar_RechazaCodigoDeOtroSecreto()
    {
        var secretoA = _sut.GenerarSecreto();
        var secretoB = _sut.GenerarSecreto();
        var codigoB = _sut.GenerarCodigoActual(secretoB);

        _sut.Validar(secretoA, codigoB).Should().BeFalse();
    }
}
