using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.ReglasPersonalizadas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Application.ReglasPersonalizadas.Expresiones;
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
    public async Task<IActionResult> GetCatalogo(CancellationToken ct)
    {
        var etiquetasFuente = new Dictionary<string, string>
        {
            ["Factura"] = "Facturas (DCTO)",
            ["CierreTurno"] = "Cierres de turno (TURN)",
            ["DetalleFactura"] = "Despachos de combustible (DESP)",
            ["Credito"] = "Créditos (CRED_CABE)",
            ["TarjetaTurno"] = "Transacciones con tarjeta (TURN_TARJ)"
        };

        var fuentes = CatalogoReglasPersonalizadas.Fuentes
            .Select(kv => new FuenteCatalogo(
                kv.Key,
                etiquetasFuente.GetValueOrDefault(kv.Key, kv.Key),
                kv.Value.Select(c => new CampoCatalogo(c.Nombre, c.Etiqueta, c.Tipo)).ToList()))
            .ToList();

        // Fuentes configurables (tablas arbitrarias del agente): se descubren del staging y se
        // auto-documentan sus campos a partir de una fila de muestra. Así el builder de reglas
        // las ofrece automáticamente, sin escribir campos a mano.
        var conocidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Factura", "DetalleFactura", "CierreTurno", "DepositoTurno",
            "Anulacion", "Credito", "TarjetaTurno"
        };
        var tiposCustom = await _dbContext.TransaccionesStaging
            .Select(s => s.TipoTransaccion)
            .Distinct()
            .ToListAsync(ct);

        // Nombres ya presentes (conocidas + las que ya se agregaron) para no duplicar.
        var agregadas = new HashSet<string>(fuentes.Select(f => f.Nombre), StringComparer.OrdinalIgnoreCase);

        foreach (var tipo in tiposCustom.Where(t => !conocidas.Contains(t)).OrderBy(t => t))
        {
            var muestra = await _dbContext.TransaccionesStaging
                .Where(s => s.TipoTransaccion == tipo)
                .OrderByDescending(s => s.Id)
                .Select(s => s.DataJson)
                .FirstOrDefaultAsync(ct);

            fuentes.Add(new FuenteCatalogo(tipo, $"{tipo} (tabla configurable)", InferirCampos(muestra)));
            agregadas.Add(tipo);
        }

        // Fuentes registradas en el catálogo CENTRAL (Reglas → "Fuentes de datos"): aparecen en el
        // builder aunque el agente todavía no haya enviado filas. Si ya llegaron datos al staging,
        // sus campos se documentan solos arriba; si no, se ofrecen los campos del esquema reportado
        // por el agente (tabla esquemas_tabla). Así el ingeniero puede crear reglas de inmediato.
        var fuentesCentrales = await _dbContext.FuentesDatos
            .AsNoTracking()
            .Where(f => f.Activa)
            .OrderBy(f => f.Nombre)
            .ToListAsync(ct);

        foreach (var fc in fuentesCentrales.Where(f => !agregadas.Contains(f.Nombre)))
        {
            var campos = await CamposDesdeEsquemaAsync(fc.Tabla, ct);
            fuentes.Add(new FuenteCatalogo(fc.Nombre, $"{fc.Nombre} ({fc.Tabla})", campos));
            agregadas.Add(fc.Nombre);
        }

        var catalogo = new CatalogoReglasResponse
        {
            Fuentes = fuentes,
            OperadoresNumero = CatalogoReglasPersonalizadas.OperadoresNumero,
            OperadoresTexto = CatalogoReglasPersonalizadas.OperadoresTexto,
            Funciones = CatalogoReglasPersonalizadas.Funciones
        };
        return Ok(catalogo);
    }

    /// <summary>
    /// Campos de una tabla a partir del esquema que reportó el agente (tabla esquemas_tabla).
    /// Así una fuente registrada en el central ya ofrece sus campos en el builder, sin esperar a
    /// que llegue la primera fila al staging.
    /// </summary>
    private async Task<List<CampoCatalogo>> CamposDesdeEsquemaAsync(string tabla, CancellationToken ct)
    {
        var t = tabla.Trim().ToUpperInvariant();
        var fila = await _dbContext.EsquemasTabla.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Tabla == t, ct);
        if (fila is null) return [];

        try
        {
            var columnas = JsonSerializer.Deserialize<List<ColumnaEsquemaRaw>>(fila.ColumnasJson, JsonOpts) ?? [];
            return columnas.Select(c => new CampoCatalogo(
                c.Nombre,
                c.Nombre,
                EsTipoNumerico(c.Tipo)
                    ? CatalogoReglasPersonalizadas.TipoNumero
                    : CatalogoReglasPersonalizadas.TipoTexto)).ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>true si el tipo de Firebird es numérico (para clasificar el campo en el builder).</summary>
    private static bool EsTipoNumerico(string? tipo)
    {
        var t = (tipo ?? "").ToUpperInvariant();
        return t.Contains("INT") || t.Contains("DOUBLE") || t.Contains("FLOAT")
               || t.Contains("NUMERIC") || t.Contains("DECIMAL");
    }

    /// <summary>Forma de una columna en el JSON del esquema reportado.</summary>
    private sealed record ColumnaEsquemaRaw(string Nombre, string Tipo, int Longitud, bool Nullable);

    /// <summary>Infiere los campos (nombre + tipo) de una fuente configurable desde una fila JSON.</summary>
    private static List<CampoCatalogo> InferirCampos(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (raw is null) return [];
            return raw.Select(kv => new CampoCatalogo(
                kv.Key,
                kv.Key,
                kv.Value.ValueKind == JsonValueKind.Number
                    ? CatalogoReglasPersonalizadas.TipoNumero
                    : CatalogoReglasPersonalizadas.TipoTexto)).ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Valida (sin guardar) una expresión del modo avanzado contra una fuente.
    /// Sirve para el comprobador en vivo del builder.
    /// </summary>
    [HttpPost("validar-expresion")]
    [ProducesResponseType(typeof(ValidarExpresionResponse), StatusCodes.Status200OK)]
    public IActionResult ValidarExpresion([FromBody] ValidarExpresionRequest request)
    {
        var errores = EvaluadorExpresion.Validar(request.Expresion ?? "", request.FuenteDatos ?? "");
        return Ok(new ValidarExpresionResponse(errores.Count == 0, errores));
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
            request.RiesgoBase,
            request.Ambito);
        regla.Activa = request.Activa;
        regla.ExpresionAvanzada = string.IsNullOrWhiteSpace(request.ExpresionAvanzada)
            ? null : request.ExpresionAvanzada.Trim();

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
        regla.ExpresionAvanzada = string.IsNullOrWhiteSpace(request.ExpresionAvanzada)
            ? null : request.ExpresionAvanzada.Trim();
        regla.RiesgoBase = request.RiesgoBase;
        regla.Ambito = ReglaPersonalizada.NormalizarAmbito(request.Ambito);
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

        if (request.RiesgoBase is < 1 or > 100)
            errores.Add("El riesgo base debe estar entre 1 y 100.");

        // Se aceptan las 5 fuentes del catálogo y también las fuentes configurables (tablas
        // arbitrarias enviadas por el agente). Solo se exige que se indique una.
        if (string.IsNullOrWhiteSpace(request.FuenteDatos))
            errores.Add("Debe indicar una fuente de datos.");

        if (!string.IsNullOrWhiteSpace(request.ExpresionAvanzada))
        {
            // Modo avanzado: validar la expresión lógica
            errores.AddRange(EvaluadorExpresion.Validar(request.ExpresionAvanzada, request.FuenteDatos));
            // La agregación sigue siendo opcional sobre el resultado del filtro
            if (request.Agregacion is not null)
                errores.AddRange(CatalogoReglasPersonalizadas.ValidarDefinicion(
                    request.FuenteDatos, [], request.Agregacion, request.RiesgoBase));
        }
        else
        {
            // Modo básico: validar condiciones + agregación
            errores.AddRange(CatalogoReglasPersonalizadas.ValidarDefinicion(
                request.FuenteDatos, request.Condiciones, request.Agregacion, request.RiesgoBase));
        }

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
        ExpresionAvanzada = regla.ExpresionAvanzada,
        RiesgoBase = regla.RiesgoBase,
        Ambito = regla.Ambito,
        Activa = regla.Activa
    };
}
