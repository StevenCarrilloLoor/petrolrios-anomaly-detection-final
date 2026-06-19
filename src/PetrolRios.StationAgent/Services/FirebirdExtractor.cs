using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Extrae transacciones de la base Firebird local usando Dapper.
/// Solo lectura — nunca modifica la base de datos de la estación.
/// Lee la cadena de conexión del <see cref="AgentConfigStore"/> en cada uso,
/// de modo que ajustar los parámetros de Firebird desde la interfaz aplica al vuelo.
/// </summary>
public sealed class FirebirdExtractor
{
    private readonly AgentConfigStore _config;
    private readonly ILogger<FirebirdExtractor> _logger;

    public FirebirdExtractor(AgentConfigStore config, ILogger<FirebirdExtractor> logger)
    {
        _config = config;
        _logger = logger;
    }

    private FbConnection CreateConnection() =>
        new(_config.Actual.ConstruirFirebirdConnectionString());

    /// <summary>
    /// Prueba la conexión a la base Firebird local (panel de control del agente).
    /// Devuelve el total de documentos en DCTO si la conexión es exitosa.
    /// </summary>
    public async Task<(bool Ok, string Mensaje, long? TotalDocumentos)> ProbarConexionAsync(CancellationToken ct)
    {
        try
        {
            using var connection = CreateConnection();
            var total = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition("SELECT COUNT(*) FROM DCTO", cancellationToken: ct));
            return (true, $"Conexión exitosa — {total:N0} documentos en DCTO", total);
        }
        catch (Exception ex)
        {
            return (false, InterpretarError(ex), null);
        }
    }

    /// <summary>Traduce errores comunes de Firebird a mensajes accionables en campo.</summary>
    private static string InterpretarError(Exception ex)
    {
        var m = ex.Message;
        if (m.Contains("user name and password", StringComparison.OrdinalIgnoreCase))
            return "Usuario/contraseña de Firebird incorrectos, o WireCrypt mal configurado " +
                   "(use 'Disabled' para Firebird 2.5, 'Enabled' para Firebird 3+).";
        if (m.Contains("unavailable", StringComparison.OrdinalIgnoreCase) ||
            m.Contains("connection", StringComparison.OrdinalIgnoreCase))
            return "No se pudo conectar al servidor Firebird. Verifique host, puerto y que el servicio esté encendido.";
        if (m.Contains("No such file", StringComparison.OrdinalIgnoreCase) ||
            m.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return "No se encontró el archivo de base de datos. Verifique la ruta de CONTAC.FDB.";
        return m;
    }

    /// <summary>
    /// Extrae todas las transacciones nuevas desde la marca de agua indicada.
    /// Retorna un diccionario: TipoTransaccion → lista de objetos serializados a JSON.
    /// </summary>
    public async Task<List<TransaccionBatchItem>> ExtractSinceAsync(
        DateTime watermark, IReadOnlyList<FuenteExtraccion>? fuentesCentrales, CancellationToken ct)
    {
        var items = new List<TransaccionBatchItem>();

        // Abrir UNA sola conexión para todo el ciclo. Si la conexión falla
        // (WireCrypt, credenciales, archivo no encontrado, servicio caído), la
        // excepción se PROPAGA para que el ciclo la reporte como ERROR. Antes cada
        // consulta abría su conexión y tragaba el error devolviendo vacío, por lo
        // que un Firebird caído se veía como "0 transacciones / OK" (punto ciego).
        using var connection = CreateConnection();
        try
        {
            await connection.OpenAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo abrir la conexión a Firebird");
            throw new InvalidOperationException($"Firebird: {InterpretarError(ex)}", ex);
        }

        items.AddRange(await ExtractTypeAsync<FacturaDto>(connection, "Factura", GetFacturasSql, watermark, r => r.FechaDocumento, ct));
        items.AddRange(await ExtractTypeAsync<DetalleFacturaDto>(connection, "DetalleFactura", GetDetallesSql, watermark, r => r.FechaDespacho, ct));
        items.AddRange(await ExtractTypeAsync<CierreTurnoDto>(connection, "CierreTurno", GetCierresSql, watermark, r => r.FechaFin, ct));
        items.AddRange(await ExtractTypeAsync<DepositoTurnoDto>(connection, "DepositoTurno", GetDepositosSql, watermark, r => r.FechaDeposito, ct));
        items.AddRange(await ExtractTypeAsync<AnulacionDto>(connection, "Anulacion", GetAnulacionesSql, watermark, r => r.FechaAnulacion, ct));
        items.AddRange(await ExtractTypeAsync<CreditoDto>(connection, "Credito", GetCreditosSql, watermark, r => r.FechaCabecera, ct));
        items.AddRange(await ExtractTypeAsync<TarjetaTurnoDto>(connection, "TarjetaTurno", GetTarjetasSql, watermark, _ => DateTime.UtcNow, ct));

        // Fuentes de extracción configurables (multi-tabla). Se combinan dos orígenes:
        //   1) El catálogo CENTRAL (registrado una sola vez por el ingeniero en el servidor;
        //      llega vía parámetro). Es el camino recomendado y escalable.
        //   2) Las fuentes LOCALES del panel del agente (respaldo / casos puntuales).
        // Se deduplican por tabla (el central tiene prioridad). Cada agente verifica que la
        // tabla y la columna existan en SU base antes de extraer, así que una fuente que no
        // aplique a esta estación simplemente se omite. Se toleran fallos individuales: una
        // fuente mal configurada no rompe el ciclo.
        var efectivas = new Dictionary<string, FuenteExtraccion>(StringComparer.OrdinalIgnoreCase);
        if (fuentesCentrales is not null)
            foreach (var f in fuentesCentrales.Where(f => f.Activa && !string.IsNullOrWhiteSpace(f.Tabla)))
                efectivas[f.Tabla] = f;
        foreach (var f in _config.Actual.FuentesExtraccion.Where(f => f.Activa && !string.IsNullOrWhiteSpace(f.Tabla)))
            efectivas.TryAdd(f.Tabla, f); // no pisa una ya definida por el central

        foreach (var fuente in efectivas.Values)
        {
            try
            {
                items.AddRange(await ExtractFuenteAsync(fuente, watermark, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extrayendo la fuente configurable '{Fuente}'", fuente.Nombre);
            }
        }

        _logger.LogInformation("Extraídas {Count} transacciones desde watermark {Watermark:O}", items.Count, watermark);
        return items;
    }

    private async Task<IEnumerable<TransaccionBatchItem>> ExtractTypeAsync<T>(
        FbConnection connection, string tipoTransaccion, string sql, DateTime watermark, Func<T, DateTime> fechaSelector, CancellationToken ct)
    {
        try
        {
            var rows = await connection.QueryAsync<T>(
                new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));

            return rows.Select(row => new TransaccionBatchItem
            {
                TipoTransaccion = tipoTransaccion,
                DataJson = System.Text.Json.JsonSerializer.Serialize(row),
                FechaOriginal = fechaSelector(row)
            });
        }
        catch (Exception ex)
        {
            // Falla de UNA tabla/consulta (p. ej. tabla ausente): se tolera y se
            // sigue con las demás. La conexión ya está validada arriba.
            _logger.LogError(ex, "Error extrayendo {Tipo} desde Firebird", tipoTransaccion);
            return [];
        }
    }

    // ─────────── Autodetección de Firebird ───────────

    /// <summary>
    /// Busca automáticamente una base Firebird CONTAC.FDB probando combinaciones comunes de
    /// host, puerto, ruta y WireCrypt (con las credenciales ya configuradas). Hace un pre-chequeo
    /// TCP del puerto para descartar rápido lo que no responde. Devuelve la primera combinación
    /// que conecta y consulta el catálogo correctamente.
    /// </summary>
    public async Task<(bool Ok, string? Host, int Port, string? Database, string Mensaje)> AutodetectarAsync(CancellationToken ct)
    {
        var cfg = _config.Actual;

        var hosts = new[] { cfg.FirebirdHost, "localhost", "127.0.0.1" }
            .Where(h => !string.IsNullOrWhiteSpace(h)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var ports = new[] { cfg.FirebirdPort, 3050, 3051 }.Where(p => p > 0).Distinct().ToArray();
        var rutas = RutasCandidatas(cfg.FirebirdDatabase);
        var wirecrypts = new[] { cfg.FirebirdWireCrypt, "Disabled", "Enabled" }
            .Where(w => !string.IsNullOrWhiteSpace(w)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var host in hosts)
        {
            foreach (var port in ports)
            {
                if (!await PuertoAbiertoAsync(host, port, 1000, ct)) continue;

                foreach (var ruta in rutas)
                {
                    foreach (var wc in wirecrypts)
                    {
                        ct.ThrowIfCancellationRequested();
                        var cs = $"User={cfg.FirebirdUser};Password={cfg.FirebirdPassword};" +
                                 $"Database={ruta};DataSource={host};Port={port};Dialect={cfg.FirebirdDialect};" +
                                 $"Charset={cfg.FirebirdCharset};WireCrypt={wc};ReadOnly=true;ConnectionTimeout=4";
                        try
                        {
                            using var conn = new FbConnection(cs);
                            await conn.OpenAsync(ct);
                            await conn.ExecuteScalarAsync<int>(
                                new CommandDefinition("SELECT COUNT(*) FROM RDB$RELATIONS", cancellationToken: ct));
                            return (true, host, port, ruta,
                                $"Firebird encontrado en {host}:{port} — {ruta} (WireCrypt {wc}).");
                        }
                        catch { /* probar siguiente combinación */ }
                    }
                }
            }
        }

        return (false, null, 0, null,
            "No se encontró una base CONTAC.FDB en las ubicaciones comunes. Indique la ruta manualmente.");
    }

    /// <summary>Rutas candidatas de CONTAC.FDB según el sistema operativo (más la ya configurada).</summary>
    private static IReadOnlyList<string> RutasCandidatas(string rutaConfigurada)
    {
        var candidatas = new List<string>();
        if (!string.IsNullOrWhiteSpace(rutaConfigurada)) candidatas.Add(rutaConfigurada);

        if (OperatingSystem.IsWindows())
        {
            candidatas.AddRange([
                @"C:\CONTAC\CONTAC.FDB", @"C:\Contaplus\CONTAC.FDB",
                @"C:\Contaplus\Datos\CONTAC.FDB", @"C:\PROGRA~1\CONTAC\CONTAC.FDB",
                @"C:\Datos\CONTAC.FDB", @"C:\Firebird\CONTAC.FDB"
            ]);
        }
        else
        {
            candidatas.AddRange([
                "/opt/firebird/data/CONTAC.FDB", "/var/lib/firebird/3.0/data/CONTAC.FDB",
                "/var/lib/firebird/data/CONTAC.FDB", "/firebird/data/CONTAC.FDB",
                "/data/CONTAC.FDB"
            ]);
        }
        return candidatas.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Pre-chequeo TCP: ¿hay algo escuchando en host:port dentro del timeout?</summary>
    private static async Task<bool> PuertoAbiertoAsync(string host, int port, int timeoutMs, CancellationToken ct)
    {
        try
        {
            using var tcp = new System.Net.Sockets.TcpClient();
            var conectar = tcp.ConnectAsync(host, port, ct).AsTask();
            var completado = await Task.WhenAny(conectar, Task.Delay(timeoutMs, ct));
            return completado == conectar && tcp.Connected;
        }
        catch
        {
            return false;
        }
    }

    // ─────────── Introspección / auto-documentación del esquema ───────────

    /// <summary>
    /// Lista las tablas de usuario de la base Firebird (excluye vistas y tablas del sistema).
    /// Base de la "documentación automática": permite elegir cualquier tabla para analizar.
    /// </summary>
    public async Task<IReadOnlyList<string>> ListarTablasAsync(CancellationToken ct)
    {
        using var connection = CreateConnection();
        const string sql = """
            SELECT TRIM(RDB$RELATION_NAME) AS NOMBRE
            FROM RDB$RELATIONS
            WHERE RDB$VIEW_BLR IS NULL
              AND (RDB$SYSTEM_FLAG IS NULL OR RDB$SYSTEM_FLAG = 0)
            ORDER BY RDB$RELATION_NAME
            """;
        var rows = await connection.QueryAsync<string>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.Select(r => r.Trim()).ToList();
    }

    /// <summary>
    /// Describe una tabla: verifica que exista y devuelve sus columnas con tipo, longitud y
    /// nulabilidad (auto-documentación). El nombre se valida contra la lista real de tablas
    /// (verificación de existencia + lista blanca anti-inyección antes de tocar el catálogo).
    /// </summary>
    public async Task<DescripcionTabla> DescribirTablaAsync(string tabla, CancellationToken ct)
    {
        var nombre = (tabla ?? string.Empty).Trim().ToUpperInvariant();
        var tablas = await ListarTablasAsync(ct);
        if (!tablas.Any(t => t.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            return new DescripcionTabla(nombre, false, [], 0);

        using var connection = CreateConnection();
        const string sql = """
            SELECT TRIM(RF.RDB$FIELD_NAME) AS NOMBRE, F.RDB$FIELD_TYPE AS TIPO,
                   F.RDB$FIELD_LENGTH AS LONGITUD, F.RDB$FIELD_SUB_TYPE AS SUBTIPO,
                   F.RDB$FIELD_SCALE AS ESCALA, RF.RDB$NULL_FLAG AS NOTNULL
            FROM RDB$RELATION_FIELDS RF
            JOIN RDB$FIELDS F ON RF.RDB$FIELD_SOURCE = F.RDB$FIELD_NAME
            WHERE RF.RDB$RELATION_NAME = @t
            ORDER BY RF.RDB$FIELD_POSITION
            """;
        var raw = await connection.QueryAsync<ColumnaRaw>(
            new CommandDefinition(sql, new { t = nombre }, cancellationToken: ct));

        var columnas = raw.Select(c => new ColumnaFirebird(
            (c.NOMBRE ?? "").Trim(),
            MapearTipoFirebird(c.TIPO, c.SUBTIPO, c.ESCALA, c.LONGITUD),
            c.LONGITUD ?? 0,
            c.NOTNULL is null)).ToList();

        long totalFilas = 0;
        try
        {
            totalFilas = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition($"SELECT COUNT(*) FROM \"{nombre}\"", cancellationToken: ct));
        }
        catch { /* el conteo es informativo; si falla, queda en 0 */ }

        return new DescripcionTabla(nombre, true, columnas, totalFilas);
    }

    /// <summary>Traduce el código de tipo de Firebird (RDB$FIELD_TYPE) a un nombre legible.</summary>
    private static string MapearTipoFirebird(short? tipo, short? subtipo, short? escala, short? longitud)
    {
        var esDecimal = escala is < 0;
        return tipo switch
        {
            7 => esDecimal ? "NUMERIC" : "SMALLINT",
            8 => esDecimal ? "NUMERIC" : "INTEGER",
            16 => esDecimal ? "DECIMAL/NUMERIC" : "BIGINT",
            10 => "FLOAT",
            27 => "DOUBLE PRECISION",
            12 => "DATE",
            13 => "TIME",
            35 => "TIMESTAMP",
            14 => $"CHAR({longitud})",
            37 => $"VARCHAR({longitud})",
            261 => subtipo == 1 ? "BLOB (texto)" : "BLOB",
            _ => $"tipo {tipo}"
        };
    }

    /// <summary>
    /// Extrae filas de una fuente configurable (tabla elegida desde el panel). Valida la tabla y
    /// la columna de watermark contra el catálogo real (lista blanca anti-inyección). Si hay
    /// columna de watermark, filtra las filas posteriores a la marca; si no, toma un tope de filas.
    /// Cada fila se serializa a JSON y se envía con el nombre de la fuente como tipo.
    /// </summary>
    public async Task<IReadOnlyList<TransaccionBatchItem>> ExtractFuenteAsync(
        FuenteExtraccion fuente, DateTime watermark, CancellationToken ct)
    {
        var tabla = (fuente.Tabla ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(tabla))
            return [];

        var tablas = await ListarTablasAsync(ct);
        if (!tablas.Any(t => t.Equals(tabla, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"La tabla '{fuente.Tabla}' no existe en la base.");

        using var connection = CreateConnection();
        string sql;
        object? parametros = null;

        var colWm = (fuente.ColumnaWatermark ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(colWm))
        {
            var desc = await DescribirTablaAsync(tabla, ct);
            var columna = desc.Columnas.FirstOrDefault(c =>
                c.Nombre.Equals(colWm, StringComparison.OrdinalIgnoreCase));
            if (columna is null)
                throw new InvalidOperationException(
                    $"La columna de watermark '{colWm}' no existe en la tabla {tabla}.");

            // Nombre de columna validado contra el catálogo: seguro para interpolar.
            sql = $"SELECT * FROM \"{tabla}\" WHERE \"{columna.Nombre}\" > @wm ORDER BY \"{columna.Nombre}\"";
            parametros = new { wm = watermark };
        }
        else
        {
            // Sin watermark: tope de filas para no volcar tablas enormes (la idempotencia del
            // central descarta los reenvíos en ciclos sucesivos).
            sql = $"SELECT FIRST 500 * FROM \"{tabla}\"";
        }

        var filas = await connection.QueryAsync(new CommandDefinition(sql, parametros, cancellationToken: ct));
        return filas
            .Select(fila => new TransaccionBatchItem
            {
                TipoTransaccion = string.IsNullOrWhiteSpace(fuente.Nombre) ? tabla : fuente.Nombre.Trim(),
                DataJson = System.Text.Json.JsonSerializer.Serialize((IDictionary<string, object>)fila),
                FechaOriginal = DateTime.UtcNow
            })
            .ToList();
    }

    #region SQL Queries (idénticas al servidor — tablas reales de Contaplus)

    private const string GetFacturasSql = """
        SELECT
            SEC_DCTO  AS SecuenciaDocumento, TIP_DCTO  AS TipoDocumento,
            NUM_DCTO  AS NumeroDocumento,    FEC_DCTO  AS FechaDocumento,
            COD_CLIE  AS CodigoCliente,      TNI_DCTO  AS TotalNeto,
            TSI_DCTO  AS TotalSinIva,        DSC_DCTO  AS Descuento,
            IVA_DCTO  AS Iva,                COD_VEND  AS CodigoVendedor,
            COD_PAGO  AS CodigoPago,         PLA_DCTO  AS Placa,
            RUC_DCTO  AS RucCliente,         NUM_TURN  AS NumeroTurno,
            SUB_DCTO  AS Subtotal,           NUM_CONS  AS NumeroConsecutivo,
            COD_CHOF  AS CodigoChofer,       COD_MANG  AS CodigoManguera
        FROM DCTO WHERE FEC_DCTO > @Watermark ORDER BY FEC_DCTO
        """;

    private const string GetDetallesSql = """
        SELECT
            NUM_DESP  AS NumeroDespacho,  COD_MANG  AS CodigoManguera,
            FIN_DESP  AS FechaDespacho,   VTO_DESP  AS VolumenTotal,
            CAN_DESP  AS Cantidad,        VUN_DESP  AS ValorUnitario,
            COD_PROD  AS CodigoProducto,  NOM_PROD  AS NombreProducto,
            COD_CLIE  AS CodigoCliente,   FAC_DESP  AS Facturado
        FROM DESP WHERE FIN_DESP > @Watermark ORDER BY FIN_DESP
        """;

    private const string GetCierresSql = """
        SELECT
            NUM_TURN  AS NumeroTurno,   COD_VEND  AS CodigoVendedor,
            FIN_TURN  AS FechaInicio,   FFI_TURN  AS FechaFin,
            SIN_TURN  AS SaldoInicial,  ING_TURN  AS Ingresos,
            EGR_TURN  AS Egresos,       SFI_TURN  AS SaldoFinal,
            FAL_TURN  AS Faltante,      SOB_TURN  AS Sobrante,
            CRE_TURN  AS Creditos,      EST_TURN  AS EstadoTurno
        FROM TURN
        WHERE FFI_TURN > @Watermark OR EST_TURN = '0'
        ORDER BY FIN_TURN
        """;

    private const string GetDepositosSql = """
        SELECT
            td.NUM_TUDP  AS NumeroDeposito,  td.COD_VEND  AS CodigoVendedor,
            td.NUM_TURN  AS NumeroTurno,     td.FEC_TUDP  AS FechaDeposito,
            td.TIP_TUDP  AS TipoDeposito,    td.DET_TUDP  AS Detalle,
            td.CAN_TUDP  AS Cantidad,        td.VAL_TUDP  AS Valor,
            td.TOT_TUDP  AS Total
        FROM TURN_DEPO td
        INNER JOIN TURN t ON td.NUM_TURN = t.NUM_TURN
        WHERE t.FFI_TURN > @Watermark ORDER BY td.FEC_TUDP
        """;

    private const string GetAnulacionesSql = """
        SELECT
            NUMAN            AS NumeroAnulacion,  TIPOCOMPROBANTE  AS TipoComprobante,
            FECHAANULACION   AS FechaAnulacion,   ESTABLECIMIENTO  AS Establecimiento,
            PUNTOEMISION     AS PuntoEmision,      SECUENCIALINICIO AS SecuencialInicio,
            SECUENCIALFIN    AS SecuencialFin,     AUTORIZACION     AS Autorizacion
        FROM ANUL WHERE FECHAANULACION > @Watermark ORDER BY FECHAANULACION
        """;

    private const string GetCreditosSql = """
        SELECT
            NUM_CABE  AS NumeroCabecera,  FEC_CABE  AS FechaCabecera,
            COD_CRED  AS CodigoCredito,   COD_SOCI  AS CodigoSocio,
            PLA_CABE  AS PlazoCabecera,   TAZ_CRED  AS TasaCredito,
            COD_GARA  AS CodigoGarante,   TCR_CABE  AS TotalCredito,
            TIN_CABE  AS TotalInteres,    COD_BANC  AS CodigoBanco,
            NUMMCOMP  AS NumeroComprobante
        FROM CRED_CABE WHERE FEC_CABE > @Watermark ORDER BY FEC_CABE
        """;

    private const string GetTarjetasSql = """
        SELECT
            tt.NUM_TURN_TARJ  AS NumeroTarjetaTurno,  tt.NUM_TURN  AS NumeroTurno,
            tt.COD_BANC       AS CodigoBanco,          tt.CAN_TURN_TARJ AS Cantidad,
            tt.VAL_TURN_TARJ  AS Valor
        FROM TURN_TARJ tt
        INNER JOIN TURN t ON tt.NUM_TURN = t.NUM_TURN
        WHERE t.FFI_TURN > @Watermark ORDER BY t.FFI_TURN
        """;

    #endregion
}

/// <summary>
/// Representa una transacción individual dentro de un lote de envío.
/// </summary>
public sealed class TransaccionBatchItem
{
    public required string TipoTransaccion { get; set; }
    public required string DataJson { get; set; }
    public required DateTime FechaOriginal { get; set; }
}

/// <summary>Fila cruda del catálogo de Firebird (RDB$RELATION_FIELDS).</summary>
internal sealed class ColumnaRaw
{
    public string? NOMBRE { get; set; }
    public short? TIPO { get; set; }
    public short? LONGITUD { get; set; }
    public short? SUBTIPO { get; set; }
    public short? ESCALA { get; set; }
    public short? NOTNULL { get; set; }
}

/// <summary>Una columna documentada automáticamente: nombre, tipo legible, longitud y nulabilidad.</summary>
public sealed record ColumnaFirebird(string Nombre, string Tipo, int Longitud, bool Nullable);

/// <summary>Documentación automática de una tabla: si existe, sus columnas y su conteo de filas.</summary>
public sealed record DescripcionTabla(
    string Tabla, bool Existe, IReadOnlyList<ColumnaFirebird> Columnas, long TotalFilas);
