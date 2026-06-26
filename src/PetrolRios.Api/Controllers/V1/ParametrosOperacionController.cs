using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Jobs;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Parámetros de operación del sistema (solo Administrador): el nivel mínimo de alerta que dispara
/// aviso por correo y la frecuencia (cron) del job de detección. Se persisten en config/operacion.json
/// sin recompilar. El nivel se aplica en el siguiente ciclo; el cron se re-registra al guardar (en vivo).
/// </summary>
[ApiController]
[Route("api/v1/operacion")]
[Authorize(Roles = "Administrador", Policy = "Central")]
public sealed class ParametrosOperacionController : ControllerBase
{
    private readonly IParametrosOperacion _store;

    public ParametrosOperacionController(IParametrosOperacion store)
    {
        _store = store;
    }

    /// <summary>Valores actuales de los parámetros de operación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(OperacionConfig), StatusCodes.Status200OK)]
    public ActionResult<OperacionConfig> Actual() => Ok(_store.Actual());

    /// <summary>Valida, persiste y aplica los parámetros (re-registra el job con el nuevo cron).</summary>
    [HttpPut]
    [ProducesResponseType(typeof(OperacionConfig), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<OperacionConfig> Guardar([FromBody] OperacionConfig config)
    {
        if (!Enum.TryParse<NivelRiesgo>(config.NivelMinimoCorreo, true, out _))
            return BadRequest(new { mensaje = "Nivel inválido. Use Bajo, Medio, Alto o Critico." });

        var cron = config.CronExpression?.Trim() ?? "";
        if (cron.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 5)
            return BadRequest(new
            {
                mensaje = "La frecuencia (cron) debe tener 5 campos (p. ej. \"*/5 * * * *\" = cada 5 minutos). " +
                          "Mejor elige una opción de la lista en Ajustes."
            });

        // Aplicar el nuevo cron re-registrando el job. Hangfire PARSEA el cron aquí y lanza si es inválido
        // ANTES de tocar el almacenamiento, así que lo registramos primero dentro de try/catch: si el cron
        // está mal, devolvemos 400 sin persistir ni dejar el job en mal estado (el anterior sigue vigente).
        try
        {
            RecurringJob.AddOrUpdate<AnomalyDetectionJob>(
                "anomaly-detection",
                job => job.ExecuteAsync(CancellationToken.None),
                cron);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                mensaje = "La frecuencia no es válida (" + ex.Message + "). " +
                          "Elige una opción de la lista, o revisa la expresión cron de 5 campos."
            });
        }

        // Cron válido y job aplicado: ahora sí persistimos (con el cron ya normalizado/recortado).
        _store.Guardar(new OperacionConfig(config.NivelMinimoCorreo, cron));

        return Ok(_store.Actual());
    }
}
