namespace PetrolRios.Application.DTOs.Configuracion;

/// <summary>Estado de la conexión activa a la base, para mostrarlo en Ajustes (sin exponer la contraseña).</summary>
public sealed record ConexionActiva(
    string Fuente,
    bool EditableDesdeUi,
    string Enmascarada,
    string? Servidor,
    int Puerto,
    string? BaseDatos,
    string? Usuario,
    string ModoSsl);

/// <summary>
/// Petición para probar o guardar una conexión. Se acepta una <see cref="Cadena"/> cruda
/// (modo avanzado, cualquier método de conexión) o los campos simples del servidor.
/// </summary>
public sealed record ProbarConexionRequest(
    string? Cadena,
    string? Servidor,
    int? Puerto,
    string? BaseDatos,
    string? Usuario,
    string? Password,
    string? ModoSsl);

public sealed record ProbarConexionResponse(bool Ok, string? Mensaje, string? Version);

public sealed record GuardarConexionResponse(bool Ok, string Mensaje);
