using PetrolRios.Application.DTOs.Configuracion;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Lee y persiste los parámetros de operación (nivel mínimo de correo + cron del job) en
/// config/operacion.json, sin tocar el código. El nivel de correo se aplica en el siguiente ciclo
/// de detección; el cron, al re-registrar el job recurrente (en vivo, sin reiniciar).
/// </summary>
public interface IParametrosOperacion
{
    /// <summary>Valores actuales (del archivo si existe, o los de appsettings/por defecto).</summary>
    OperacionConfig Actual();

    /// <summary>Persiste los parámetros (normaliza vacíos a los valores por defecto).</summary>
    void Guardar(OperacionConfig config);
}
