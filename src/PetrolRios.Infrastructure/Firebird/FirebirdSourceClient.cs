using System.Data;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Firebird;

/// <summary>
/// Implementación de solo lectura contra una base Firebird (CONTAC.FDB) usando Dapper.
/// Cada instancia opera sobre una estación específica.
/// </summary>
public sealed class FirebirdSourceClient : IFirebirdSourceClient
{
    private readonly string _connectionString;

    public FirebirdSourceClient(string connectionString)
    {
        _connectionString = connectionString;
    }

    private FbConnection CreateConnection()
    {
        var connection = new FbConnection(_connectionString);
        return connection;
    }

    public async Task<IReadOnlyList<FacturaDto>> GetFacturasDesdeAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                SEC_DCTO  AS SecuenciaDocumento,
                TIP_DCTO  AS TipoDocumento,
                NUM_DCTO  AS NumeroDocumento,
                FEC_DCTO  AS FechaDocumento,
                COD_CLIE  AS CodigoCliente,
                TNI_DCTO  AS TotalNeto,
                TSI_DCTO  AS TotalSinIva,
                DSC_DCTO  AS Descuento,
                IVA_DCTO  AS Iva,
                COD_VEND  AS CodigoVendedor,
                COD_PAGO  AS CodigoPago,
                PLA_DCTO  AS Placa,
                RUC_DCTO  AS RucCliente,
                NUM_TURN  AS NumeroTurno,
                SUB_DCTO  AS Subtotal,
                NUM_CONS  AS NumeroConsecutivo,
                COD_CHOF  AS CodigoChofer,
                COD_MANG  AS CodigoManguera
            FROM DCTO
            WHERE FEC_DCTO > @Watermark
            ORDER BY FEC_DCTO
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<FacturaDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<DetalleFacturaDto>> GetDetallesFacturaAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                NUM_DESP  AS NumeroDespacho,
                COD_MANG  AS CodigoManguera,
                FIN_DESP  AS FechaDespacho,
                VTO_DESP  AS VolumenTotal,
                CAN_DESP  AS Cantidad,
                VUN_DESP  AS ValorUnitario,
                COD_PROD  AS CodigoProducto,
                NOM_PROD  AS NombreProducto,
                COD_CLIE  AS CodigoCliente
            FROM DESP
            WHERE FIN_DESP > @Watermark
            ORDER BY FIN_DESP
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<DetalleFacturaDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<CierreTurnoDto>> GetCierresTurnoAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                NUM_TURN  AS NumeroTurno,
                COD_VEND  AS CodigoVendedor,
                FIN_TURN  AS FechaInicio,
                FFI_TURN  AS FechaFin,
                SIN_TURN  AS SaldoInicial,
                ING_TURN  AS Ingresos,
                EGR_TURN  AS Egresos,
                SFI_TURN  AS SaldoFinal,
                FAL_TURN  AS Faltante,
                SOB_TURN  AS Sobrante,
                CRE_TURN  AS Creditos
            FROM TURN
            WHERE FFI_TURN > @Watermark
            ORDER BY FFI_TURN
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<CierreTurnoDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<DepositoTurnoDto>> GetDepositosTurnoAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                td.NUM_TUDP  AS NumeroDeposito,
                td.COD_VEND  AS CodigoVendedor,
                td.NUM_TURN  AS NumeroTurno,
                td.FEC_TUDP  AS FechaDeposito,
                td.TIP_TUDP  AS TipoDeposito,
                td.DET_TUDP  AS Detalle,
                td.CAN_TUDP  AS Cantidad,
                td.VAL_TUDP  AS Valor,
                td.TOT_TUDP  AS Total
            FROM TURN_DEPO td
            INNER JOIN TURN t ON td.NUM_TURN = t.NUM_TURN
            WHERE t.FFI_TURN > @Watermark
            ORDER BY td.FEC_TUDP
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<DepositoTurnoDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<AnulacionDto>> GetAnulacionesAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                NUMAN              AS NumeroAnulacion,
                TIPOCOMPROBANTE    AS TipoComprobante,
                FECHAANULACION     AS FechaAnulacion,
                ESTABLECIMIENTO    AS Establecimiento,
                PUNTOEMISION       AS PuntoEmision,
                SECUENCIALINICIO   AS SecuencialInicio,
                SECUENCIALFIN      AS SecuencialFin,
                AUTORIZACION       AS Autorizacion
            FROM ANUL
            WHERE FECHAANULACION > @Watermark
            ORDER BY FECHAANULACION
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<AnulacionDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<CreditoDto>> GetCreditosAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                NUM_CABE   AS NumeroCabecera,
                FEC_CABE   AS FechaCabecera,
                COD_CRED   AS CodigoCredito,
                COD_SOCI   AS CodigoSocio,
                PLA_CABE   AS PlazoCabecera,
                TAZ_CRED   AS TasaCredito,
                COD_GARA   AS CodigoGarante,
                TCR_CABE   AS TotalCredito,
                TIN_CABE   AS TotalInteres,
                COD_BANC   AS CodigoBanco,
                NUMMCOMP   AS NumeroComprobante
            FROM CRED_CABE
            WHERE FEC_CABE > @Watermark
            ORDER BY FEC_CABE
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<CreditoDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }

    public async Task<IReadOnlyList<TarjetaTurnoDto>> GetTarjetasTurnoAsync(DateTime watermark, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                tt.NUM_TURN_TARJ  AS NumeroTarjetaTurno,
                tt.NUM_TURN       AS NumeroTurno,
                tt.COD_BANC       AS CodigoBanco,
                tt.CAN_TURN_TARJ  AS Cantidad,
                tt.VAL_TURN_TARJ  AS Valor
            FROM TURN_TARJ tt
            INNER JOIN TURN t ON tt.NUM_TURN = t.NUM_TURN
            WHERE t.FFI_TURN > @Watermark
            ORDER BY t.FFI_TURN
            """;

        using var connection = CreateConnection();
        var result = await connection.QueryAsync<TarjetaTurnoDto>(
            new CommandDefinition(sql, new { Watermark = watermark }, cancellationToken: ct));
        return result.AsList();
    }
}
