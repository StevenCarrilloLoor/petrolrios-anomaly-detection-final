using System.Security.Cryptography;

namespace PetrolRios.StationAgent.Security;

/// <summary>
/// Hash de contraseñas con PBKDF2 (SHA-256, 100k iteraciones) — sin dependencias
/// externas. Se usa para la contraseña local de respaldo del panel del agente.
/// Formato almacenado: "iteraciones.saltBase64.hashBase64".
/// </summary>
public static class PasswordHasher
{
    private const int Iteraciones = 100_000;
    private const int TamSalt = 16;
    private const int TamHash = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(TamSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, TamHash);
        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string? almacenado)
    {
        if (string.IsNullOrWhiteSpace(almacenado)) return false;
        var partes = almacenado.Split('.', 3);
        if (partes.Length != 3) return false;
        if (!int.TryParse(partes[0], out var iter)) return false;

        try
        {
            var salt = Convert.FromBase64String(partes[1]);
            var esperado = Convert.FromBase64String(partes[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(actual, esperado);
        }
        catch
        {
            return false;
        }
    }
}
