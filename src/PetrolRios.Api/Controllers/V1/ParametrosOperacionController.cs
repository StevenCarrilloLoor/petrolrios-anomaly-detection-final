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
                mensaje = "Expresión cron inválida: use 5 campos (p. ej. \"*/5 * * * *\" para cada 5 minutos)."
            });

        _store.Guardar(config);

        // Aplicar el nuevo cron en vivo (sin reiniciar) re-registrando el job recurrente.
        RecurringJob.AddOrUpdate<AnomalyDetectionJob>(
            "anomaly-detection",
            job => job.ExecuteAsync(CancellationToken.None),
            cron);

        return Ok(_store.Actual());
    }
}
