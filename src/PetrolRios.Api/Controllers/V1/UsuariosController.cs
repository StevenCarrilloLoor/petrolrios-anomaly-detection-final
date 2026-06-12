using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Usuarios;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Listar todos los usuarios.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _usuarioService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Listar los auditores activos, para asignación de alertas (CU-11).
    /// Disponible también para Supervisor.
    /// </summary>
    [HttpGet("auditores")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditores(CancellationToken ct)
    {
        var usuarios = await _usuarioService.GetAllAsync(ct);
        var auditores = usuarios
            .Where(u => u.Activo && (u.Rol == "Auditor" || u.Rol == "Supervisor"))
            .Where(u => !u.Email.StartsWith("agent-", StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Ok(auditores);
    }

    /// <summary>
    /// Obtener un usuario por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _usuarioService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Crear un nuevo usuario.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CrearUsuarioRequest request, CancellationToken ct)
    {
        var result = await _usuarioService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualizar un usuario existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarUsuarioRequest request, CancellationToken ct)
    {
        var result = await _usuarioService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Desactivar un usuario (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _usuarioService.DeleteAsync(id, ct);
        return NoContent();
    }
}
