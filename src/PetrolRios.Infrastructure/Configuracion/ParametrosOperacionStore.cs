using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Configuracion;

/// <summary>
/// Persiste los parámetros de operación en config/operacion.json (igual que <see cref="ConexionStore"/>
/// para la conexión). Si el archivo no existe o está corrupto, usa los valores por defecto de
/// appsettings (Hangfire:CronExpression y Operacion:NivelMinimoCorreo) o, en su defecto, los fijos.
/// </summary>
public sealed class ParametrosOperacionStore : IParametrosOperacion
{
    private const string CronPorDefecto = "*/5 * * * *";
    private const string NivelPorDefecto = "Critico";

    private readonly string _ruta;
    private readonly IConfiguration _config;

    public ParametrosOperacionStore(string configDir, IConfiguration config)
    {
        _ruta = Path.Combine(configDir, "operacion.json");
        _config = config;
    }

    public OperacionConfig Actual()
    {
        var cronDefault = _config.GetValue<string>("Hangfire:CronExpression") ?? CronPorDefecto;
        var nivelDefault = _config.GetValue<string>("Operacion:NivelMinimoCorreo") ?? NivelPorDefecto;

        if (File.Exists(_ruta))
        {
            try
            {
                var leido = JsonSerializer.Deserialize<OperacionConfig>(File.ReadAllText(_ruta));
                if (leido is not null)
                    return new OperacionConfig(
                        string.IsNullOrWhiteSpace(leido.NivelMinimoCorreo) ? nivelDefault : leido.NivelMinimoCorreo.Trim(),
                        string.IsNullOrWhiteSpace(leido.CronExpression) ? cronDefault : leido.CronExpression.Trim());
            }
            catch
            {
                // Archivo corrupto: se ignora y se usan los valores por defecto.
            }
        }
        return new OperacionConfig(nivelDefault, cronDefault);
    }

    public void Guardar(OperacionConfig config)
    {
        var dir = Path.GetDirectoryName(_ruta);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var limpio = new OperacionConfig(
            string.IsNullOrWhiteSpace(config.NivelMinimoCorreo) ? NivelPorDefecto : config.NivelMinimoCorreo.Trim(),
            string.IsNullOrWhiteSpace(config.CronExpression) ? CronPorDefecto : config.CronExpression.Trim());

        File.WriteAllText(_ruta, JsonSerializer.Serialize(limpio, new JsonSerializerOptions { WriteIndented = true }));
    }
}
