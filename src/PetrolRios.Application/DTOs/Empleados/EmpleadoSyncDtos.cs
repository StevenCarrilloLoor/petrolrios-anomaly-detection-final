namespace PetrolRios.Application.DTOs.Empleados;

/// <summary>Un empleado/despachador reportado por el agente: código de vendedor → nombre.</summary>
public sealed record EmpleadoSyncItem(string Codigo, string Nombre);

/// <summary>Lote del catálogo de empleados que envía un agente de estación al central.</summary>
public sealed record SincronizarEmpleadosRequest(
    string CodigoEstacion,
    IReadOnlyList<EmpleadoSyncItem> Empleados);
