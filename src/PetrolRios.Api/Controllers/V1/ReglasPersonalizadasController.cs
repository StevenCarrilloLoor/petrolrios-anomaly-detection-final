using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.ReglasPersonalizadas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Reglas de negocio definidas por el usuario: el sistema es escalable sin tocar
/// código. Cada regla se valida contra el catálogo de fuentes/campos/operadores
/// antes de guardarse, y la evalúa el detector genérico en cada ciclo.
/// </summary>
[ApiController]
[Route("api/v1/reglas-personalizadas")]
[Authorize(Roles = "Supervisor,Administrador")]
public sealed class ReglasPersonalizadasController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly PetrolRiosDbContext _dbContext;
    private readonly ILogService _logService;

    public ReglasPersonalizadasController(PetrolRiosDbContext dbContext, ILogService logService)
    {
        _dbContext = dbContext;
        _logService = logService;
    }

    /// <summary>
    /// Catálogo para el builder: fuentes de datos, campos disponibles, operadores y funciones.
    /// </summary>
    [HttpGet("catalogo")]
    [ProducesResponseType(typeof(CatalogoReglasResponse), StatusCodes.Status200OK)]
    public IActionResult GetCatalogo()
    {
        var etiquetasFuente = new Dictionary<string, string>
        {
            ["Factura"] = "Facturas (DCTO)",
            ["CierreTurno"] = "Cierres de turno (TURN)",
            ["DetalleFactura"] = "Despachos de combustible (DESP)",
            ["Credito"] = "Créditos (CRED_CABE)",
            ["TarjetaTurno"] = "Transacciones con tarjeta (TURN_TARJ)"
        };

        var catalogo = new CatalogoReglasResponse
        {
            Fuentes = CatalogoReglasPersonalizadas.Fuentes
                .Select(kv => new FuenteCatalogo(
                    kv.Key,
                    etiquetasFuente.GetValueOrDefault(kv.Key, kv.Key),
                    kv.Value.Select(c => new CampoCatalogo(c.Nombre, c.Etiqueta, c.Tipo)).ToList()))
                .ToList(),
            OperadoresNumero = CatalogoReglasPersonalizadas.OperadoresNumero,
            OperadoresTexto = CatalogoReglasPersonalizadas.OperadoresTexto,
            Funciones = CatalogoReglasPersonalizadas.Funciones
        };
        return Ok(catalogo);
    }

    /// <summary>Listar todas las reglas personalizadas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReglaPersonalizadaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var reglas = await _dbContext.ReglasPersonalizadas
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .ToListAsync(ct);
        return Ok(reglas.Select(MapToResponse).ToList());
    }

    /// <summary>Crear una regla personalizada (validada contra el catálogo).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReglaPersonalizadaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] GuardarReglaPersonalizadaRequest request, CancellationToken ct)
    {
        var errores = Validar(request, out var nombre);
        if (errores.Count > 0) return BadRequest(new { errores });

        if (await _dbContext.ReglasPersonalizadas.AnyAsync(r => r.Nombre == nombre, ct))
            return BadRequest(new { errores = new[] { $"Ya existe una regla llamada '{nombre}'." } });

        var regla = ReglaPersonalizada.Create(
            nombre,
            request.Descripcion.Trim(),
            request.FuenteDatos,
            JsonSerializer.Serialize(request.Condiciones),
            request.Agregacion is null ? null : JsonSerializer.Serialize(request.Agregacion),
            request.RiesgoBase);
        regla.Activa = request.Activa;

        await _dbContext.ReglasPersonalizadas.AddAsync(regla, ct);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Creación de regla personalizada", "ReglaPersonalizada", regla.Id,
            new { regla.Nombre, regla.FuenteDatos }, ct: ct);

        return CreatedAtAction(nameof(GetAll), new { id = regla.Id }, MapToResponse(regla));
    }

    /// <summary>Actualizar una regla personalizada.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReglaPersonalizadaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] GuardarReglaPersonalizadaRequest request, CancellationToken ct)
    {
        var regla = await _dbContext.ReglasPersonalizadas.FindAsync([id], ct);
        if (regla is null) return NotFound();

        var errores = Validar(request, out var nombre);
        if (errores.Count > 0) return BadRequest(new { errores });

        if (await _dbContext.ReglasPersonalizadas.AnyAsync(r => r.Nombre == nombre && r.Id != id, ct))
            return BadRequest(new { errores = new[] { $"Ya existe otra regla llamada '{nombre}'." } });

        regla.Nombre = nombre;
        regla.Descripcion = request.Descripcion.Trim();
        regla.FuenteDatos = request.FuenteDatos;
        regla.CondicionesJson = JsonSerializer.Serialize(request.Condiciones);
        regla.AgregacionJson = request.Agregacion is null ? null : JsonSerializer.Serialize(request.Agregacion);
        regla.RiesgoBase = request.RiesgoBase;
        regla.Activa = request.Activa;
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Actualización de regla personalizada", "ReglaPersonalizada", id,
            new { regla.Nombre, regla.Activa }, ct: ct);

        return Ok(MapToResponse(regla));
    }

    /// <summary>Eliminar una regla personalizada.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var regla = await _dbContext.ReglasPersonalizadas.FindAsync([id], ct);
        if (regla is null) return NotFound();

        _dbContext.ReglasPersonalizadas.Remove(regla);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Eliminación de regla personalizada", "ReglaPersonalizada", id,
            new { regla.Nombre }, ct: ct);

        return NoContent();
    }

    private static IReadOnlyList<string> Validar(
        GuardarReglaPersonalizadaRequest request, out string nombre)
    {
        nombre = request.Nombre.Trim();
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
            errores.Add("El nombre de la regla es obligatorio.");

        errores.AddRange(CatalogoReglasPersonalizadas.ValidarDefinicion(
            request.FuenteDatos, request.Condiciones, request.Agregacion, request.RiesgoBase));

        return errores;
    }

    private static ReglaPersonalizadaResponse MapToResponse(ReglaPersonalizada regla) => new()
    {
        Id = regla.Id,
        Nombre = regla.Nombre,
        Descripcion = regla.Descripcion,
        FuenteDatos = regla.FuenteDatos,
        Condiciones = JsonSerializer.Deserialize<List<CondicionRegla>>(regla.CondicionesJson, JsonOpts) ?? [],
        Agregacion = string.IsNullOrWhiteSpace(regla.AgregacionJson)
            ? null
            : JsonSerializer.Deserialize<AgregacionRegla>(regla.AgregacionJson, JsonOpts),
        RiesgoBase = regla.RiesgoBase,
        Activa = regla.Activa
    };
}
