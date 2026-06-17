using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>
/// Reglas de seguridad de la entidad Usuario: bloqueo por intentos fallidos,
/// cambio obligatorio de contraseña y 2FA (TOTP).
/// </summary>
public class UsuarioSeguridadTests
{
    private static Usuario NuevoUsuario() =>
        Usuario.Create("u@petrolrios.com", "Usuario", "hash", 1);

    [Fact]
    public void Bloqueo_SeActivaAlSuperarElMaximoDeIntentos()
    {
        var u = NuevoUsuario();

        u.RegistrarFalloLogin(maxIntentos: 3, minutosBloqueo: 15);
        u.RegistrarFalloLogin(maxIntentos: 3, minutosBloqueo: 15);
        u.EstaBloqueado().Should().BeFalse("aún no llega al máximo");

        u.RegistrarFalloLogin(maxIntentos: 3, minutosBloqueo: 15);
        u.EstaBloqueado().Should().BeTrue("al tercer intento se bloquea");
    }

    [Fact]
    public void ResetearFallos_DesbloqueaYLimpiaContador()
    {
        var u = NuevoUsuario();
        u.RegistrarFalloLogin(1, 15);
        u.EstaBloqueado().Should().BeTrue();

        u.ResetearFallos();
        u.EstaBloqueado().Should().BeFalse();
    }

    [Fact]
    public void CambiarPassword_LimpiaDebeCambiarYDesbloquea()
    {
        var u = NuevoUsuario();
        u.DebeCambiarPassword = true;
        u.RegistrarFalloLogin(1, 15);

        u.UpdatePassword("nuevo-hash");

        u.DebeCambiarPassword.Should().BeFalse();
        u.EstaBloqueado().Should().BeFalse();
    }

    [Fact]
    public void Totp_SeConfiguraYActivaEnDosPasos()
    {
        var u = NuevoUsuario();
        u.ConfigurarTotp("SECRETO");
        u.TotpHabilitado.Should().BeFalse("configurar no activa hasta confirmar el primer código");

        u.ActivarTotp();
        u.TotpHabilitado.Should().BeTrue();

        u.DeshabilitarTotp();
        u.TotpHabilitado.Should().BeFalse();
        u.TotpSecret.Should().BeNull();
    }
}
