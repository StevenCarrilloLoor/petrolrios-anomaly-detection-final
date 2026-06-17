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
    public async Task<List<TransaccionBatchItem>> ExtractSinceAsync(DateTime watermark, CancellationToken ct)
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
            COD_CLIE  AS CodigoCliente
        FROM DESP WHERE FIN_DESP > @Watermark ORDER BY FIN_DESP
        """;

    private const string GetCierresSql = """
        SELECT
            NUM_TURN  AS NumeroTurno,   COD_VEND  AS CodigoVendedor,
            FIN_TURN  AS FechaInicio,   FFI_TURN  AS FechaFin,
            SIN_TURN  AS SaldoInicial,  ING_TURN  AS Ingresos,
            EGR_TURN  AS Egresos,       SFI_TURN  AS SaldoFinal,
            FAL_TURN  AS Faltante,      SOB_TURN  AS Sobrante,
            CRE_TURN  AS Creditos
        FROM TURN WHERE FFI_TURN > @Watermark ORDER BY FFI_TURN
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
