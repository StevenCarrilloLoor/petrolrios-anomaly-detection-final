using PetrolRios.Application.DTOs.Consultas;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Cola (en memoria) de consultas en vivo a la Firebird de las estaciones. El central no puede iniciar
/// la conexión hacia el agente (los agentes llaman al central), así que la consulta se ENCOLA aquí; el
/// agente la recoge en su próximo heartbeat (cada 1 s), la corre SOLO LECTURA y devuelve el resultado,
/// que la interfaz sondea por <see cref="Obtener"/>. Mismo patrón que <see cref="ISolicitudesEsquema"/>.
/// </summary>
public interface IConsultasFirebird
{
    /// <summary>Encola una consulta y devuelve su id (la interfaz lo usa para sondear el resultado).</summary>
    string Encolar(SolicitudConsulta solicitud);

    /// <summary>Devuelve (y marca como tomadas) las consultas pendientes de una estación, para el agente.</summary>
    IReadOnlyList<ConsultaPendiente> TomarPendientes(string codigoEstacion);

    /// <summary>El agente reporta el resultado (o el error) de una consulta.</summary>
    void Responder(string id, bool ok, string? resultadoJson, string? error);

    /// <summary>Estado/resultado actual de una consulta (para el sondeo de la interfaz); null si no existe/expiró.</summary>
    ConsultaEstado? Obtener(string id);
}
