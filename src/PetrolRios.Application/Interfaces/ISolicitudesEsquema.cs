namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Cola (en memoria) de solicitudes de "cargar esquema" por estación. El central no puede iniciar
/// una conexión hacia el agente (los agentes son quienes llaman al central), así que cuando un
/// administrador pide cargar el esquema de una estación, se marca aquí; en el próximo heartbeat de
/// ese agente se le indica que reporte su esquema y la marca se limpia.
/// </summary>
public interface ISolicitudesEsquema
{
    /// <summary>Marca que la estación debe reportar su esquema en su próximo heartbeat.</summary>
    void Solicitar(string codigoEstacion);

    /// <summary>Devuelve y limpia si hay una solicitud pendiente para esa estación.</summary>
    bool TomarPendiente(string codigoEstacion);
}
