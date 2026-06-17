using System.Security.Cryptography;
using System.Text;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Application.Security;

/// <summary>
/// Implementación de TOTP (RFC 6238) con HMAC-SHA1, ventana de 30 s y 6 dígitos —
/// los parámetros por defecto de Google Authenticator. Sin dependencias externas.
/// </summary>
public sealed class TotpService : ITotpService
{
    private const int Digitos = 6;
    private const int PeriodoSegundos = 30;
    private const string Base32Alfabeto = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string GenerarSecreto()
    {
        var bytes = RandomNumberGenerator.GetBytes(20); // 160 bits
        return Base32Encode(bytes);
    }

    public string ConstruirUriOtpauth(string secreto, string cuenta, string emisor = "PetrolRios")
    {
        var emisorEnc = Uri.EscapeDataString(emisor);
        var cuentaEnc = Uri.EscapeDataString(cuenta);
        return $"otpauth://totp/{emisorEnc}:{cuentaEnc}?secret={secreto}&issuer={emisorEnc}&digits={Digitos}&period={PeriodoSegundos}";
    }

    public bool Validar(string secreto, string codigo)
    {
        if (string.IsNullOrWhiteSpace(secreto) || string.IsNullOrWhiteSpace(codigo))
            return false;
        codigo = codigo.Trim();
        if (codigo.Length != Digitos) return false;

        var ventanaActual = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / PeriodoSegundos;
        // Tolerancia de ±1 ventana para compensar el desfase de reloj.
        for (var offset = -1; offset <= 1; offset++)
        {
            if (GenerarCodigo(secreto, ventanaActual + offset) == codigo)
                return true;
        }
        return false;
    }

    /// <summary>Código TOTP válido en este instante (útil para pruebas y diagnóstico).</summary>
    public string GenerarCodigoActual(string secreto)
    {
        var ventana = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / PeriodoSegundos;
        return GenerarCodigo(secreto, ventana);
    }

    private static string GenerarCodigo(string secretoBase32, long contador)
    {
        var clave = Base32Decode(secretoBase32);
        var bytesContador = BitConverter.GetBytes(contador);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytesContador);

        using var hmac = new HMACSHA1(clave);
        var hash = hmac.ComputeHash(bytesContador);

        var offset = hash[^1] & 0x0F;
        var binario = ((hash[offset] & 0x7F) << 24)
                    | ((hash[offset + 1] & 0xFF) << 16)
                    | ((hash[offset + 2] & 0xFF) << 8)
                    | (hash[offset + 3] & 0xFF);

        var codigo = binario % (int)Math.Pow(10, Digitos);
        return codigo.ToString().PadLeft(Digitos, '0');
    }

    private static string Base32Encode(byte[] datos)
    {
        var sb = new StringBuilder();
        int buffer = 0, bitsEnBuffer = 0;
        foreach (var b in datos)
        {
            buffer = (buffer << 8) | b;
            bitsEnBuffer += 8;
            while (bitsEnBuffer >= 5)
            {
                bitsEnBuffer -= 5;
                sb.Append(Base32Alfabeto[(buffer >> bitsEnBuffer) & 0x1F]);
            }
        }
        if (bitsEnBuffer > 0)
            sb.Append(Base32Alfabeto[(buffer << (5 - bitsEnBuffer)) & 0x1F]);
        return sb.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        base32 = base32.TrimEnd('=').ToUpperInvariant();
        var bytes = new List<byte>();
        int buffer = 0, bitsEnBuffer = 0;
        foreach (var c in base32)
        {
            var valor = Base32Alfabeto.IndexOf(c);
            if (valor < 0) continue;
            buffer = (buffer << 5) | valor;
            bitsEnBuffer += 5;
            if (bitsEnBuffer >= 8)
            {
                bitsEnBuffer -= 8;
                bytes.Add((byte)((buffer >> bitsEnBuffer) & 0xFF));
            }
        }
        return bytes.ToArray();
    }
}
