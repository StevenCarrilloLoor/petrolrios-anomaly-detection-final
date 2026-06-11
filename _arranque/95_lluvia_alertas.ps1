# PetrolRios - Lluvia de alertas
# Genera ~50 transacciones que disparan ~30 alertas via /api/v1/ingesta
# Cubre las 4 categorias de detectores de la tesis Tabla 3

$ErrorActionPreference = 'Stop'
$Api   = 'http://localhost:5170'
$Email = 'agent-est-001@petrolrios.com'
$Pwd   = 'Agent123!'

Write-Host '============================================================'
Write-Host ' LLUVIA DE ALERTAS - PowerShell directo'
Write-Host '============================================================'

# 1) Login
$loginBody = @{ email = $Email; password = $Pwd } | ConvertTo-Json -Compress
$login = Invoke-RestMethod -Uri "$Api/api/v1/auth/login" -Method Post -ContentType 'application/json' -Body $loginBody
$jwt = $login.token
Write-Host "  [1/4] JWT obtenido: $($jwt.Substring(0,40))..."

# 2) Construir lote
$now = [DateTime]::UtcNow
$t = @()

function Make-Factura($id,$emp,$placa,$turno,$pago,$monto,$mang,$offMin) {
    $f = $now.AddMinutes(-$offMin).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    return @{
        tipoTransaccion = 'Factura'
        dataJson = (@{
            SecuenciaDocumento = $id
            TipoDocumento = 'FAC'
            NumeroDocumento = ('001-001-000{0:D6}' -f $id)
            FechaDocumento = $f
            CodigoCliente = ('CLI-' + $id)
            TotalNeto = $monto
            TotalSinIva = [math]::Round($monto / 1.12, 2)
            Descuento = 0
            Iva = [math]::Round($monto * 0.12 / 1.12, 2)
            CodigoVendedor = $emp
            CodigoPago = $pago
            Placa = $placa
            RucCliente = '1700000000000'
            NumeroTurno = $turno
            Subtotal = [math]::Round($monto / 1.12, 2)
            NumeroConsecutivo = $id
            CodigoChofer = ''
            CodigoManguera = $mang
        } | ConvertTo-Json -Compress)
        fechaOriginal = $f
    }
}

function Make-Desp($id,$mang,$gal,$prod,$cli,$offMin) {
    $f = $now.AddMinutes(-$offMin).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    return @{
        tipoTransaccion = 'DetalleFactura'
        dataJson = (@{
            NumeroDespacho = $id
            CodigoManguera = $mang
            FechaDespacho = $f
            VolumenTotal = $gal
            Cantidad = $gal
            ValorUnitario = 3.5
            CodigoProducto = $prod
            NombreProducto = $prod
            CodigoCliente = $cli
        } | ConvertTo-Json -Compress)
        fechaOriginal = $f
    }
}

function Make-Turn($num,$emp,$falt,$offMin) {
    $f = $now.AddMinutes(-$offMin).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    return @{
        tipoTransaccion = 'CierreTurno'
        dataJson = (@{
            NumeroTurno = $num
            CodigoVendedor = $emp
            FechaInicio = $now.AddHours(-8).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
            FechaFin = $f
            SaldoInicial = 100.0
            Ingresos = 900.0
            Egresos = 30.0
            SaldoFinal = 970.0
            Faltante = $falt
            Sobrante = 0.0
            Creditos = 0.0
        } | ConvertTo-Json -Compress)
        fechaOriginal = $f
    }
}

function Make-Anul($id,$offMin) {
    $f = $now.AddMinutes(-$offMin).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    return @{
        tipoTransaccion = 'Anulacion'
        dataJson = (@{
            NumeroAnulacion = $id
            TipoComprobante = 'FAC'
            FechaAnulacion = $f
            Establecimiento = '001'
            PuntoEmision = '001'
            SecuencialInicio = $id
            SecuencialFin = $id
            Autorizacion = ('AUTH-' + $id)
        } | ConvertTo-Json -Compress)
        fechaOriginal = $f
    }
}

function Make-Tarj($id,$turno,$val,$offMin) {
    $f = $now.AddMinutes(-$offMin).ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    return @{
        tipoTransaccion = 'TarjetaTurno'
        dataJson = (@{
            NumeroTarjetaTurno = $id
            NumeroTurno = $turno
            CodigoBanco = 'PICHINCHA'
            Cantidad = 1
            Valor = $val
        } | ConvertTo-Json -Compress)
        fechaOriginal = $f
    }
}

# === COMPLIANCE: 4 facturas placa ZZZ999949 con galones > 5 ===
$t += Make-Factura 600101 'EMP-007' 'ZZZ999949' 60001 'EF' 28.40 'M-01' 5
$t += Make-Desp    700101 'M-01' 8.0  'EXTRA'  'CLI-600101' 5
$t += Make-Factura 600102 'EMP-008' 'ZZZ999949' 60001 'EF' 32.50 'M-02' 6
$t += Make-Desp    700102 'M-02' 9.5  'DIESEL' 'CLI-600102' 6
$t += Make-Factura 600103 'EMP-009' 'ZZZ999949' 60001 'TC' 41.20 'M-03' 7
$t += Make-Desp    700103 'M-03' 12.0 'EXTRA'  'CLI-600103' 7
$t += Make-Factura 600104 'EMP-010' 'ZZZ999949' 60002 'EF' 26.80 'M-04' 8
$t += Make-Desp    700104 'M-04' 7.5  'EXTRA'  'CLI-600104' 8

# === COMPLIANCE: 3 placas con DIESEL + EXTRA mismo dia ===
$placas = @('GAA1111','GBB2222','GCC3333')
for ($i = 0; $i -lt $placas.Count; $i++) {
    $p = $placas[$i]
    $idA = 600200 + ($i * 10)
    $idB = $idA + 1
    $t += Make-Factura $idA       'EMP-003' $p 60003 'EF' 30.00 'M-05' 9
    $t += Make-Desp    ($idA+100) 'M-05' 10.0 'DIESEL' ('CLI-' + $idA) 9
    $t += Make-Factura $idB       'EMP-003' $p 60003 'EF' 25.00 'M-06' 10
    $t += Make-Desp    ($idB+100) 'M-06' 8.0  'EXTRA'  ('CLI-' + $idB) 10
}

# === CASH FRAUD: 5 turnos con Faltante > 50 (3 del mismo EMP -> gineteo) ===
$t += Make-Turn 70001 'EMP-007' 85.0  2
$t += Make-Turn 70002 'EMP-007' 120.0 3
$t += Make-Turn 70003 'EMP-007' 95.0  4
$t += Make-Turn 70004 'EMP-008' 70.0  5
$t += Make-Turn 70005 'EMP-009' 65.0  6

# === INVOICE ANOMALY: 5 facturas placa vacia + 8 anulaciones excesivas ===
for ($i = 1; $i -le 5; $i++) {
    $t += Make-Factura (610000+$i) 'EMP-005' '' 60004 'EF' (40+$i) ('M-0'+$i) (10+$i)
}
for ($i = 1; $i -le 8; $i++) {
    $t += Make-Anul    (500000+$i) (5+$i)
    $t += Make-Factura (620000+$i) 'EMP-011' ('PLAC'+$i) 60005 'TC' (35+$i) 'M-04' (6+$i)
}

# === PAYMENT FRAUD: 4 facturas tarjeta + reversiones tardias > 30 min ===
for ($i = 1; $i -le 4; $i++) {
    $fid = 630000+$i
    $t += Make-Factura $fid          'EMP-022' ('TPF'+$i) 60006 'TC' (75+$i) 'M-06' (90+$i*10)
    $t += Make-Tarj    (800000+$i)   60006 (-(75+$i)) (5+$i)
}
$t += Make-Turn 70006 'EMP-022' 0 1

Write-Host "  [2/4] Lote preparado: $($t.Count) transacciones"

# 3) POST
$payload = @{ codigoEstacion = 'EST-001'; transacciones = $t } | ConvertTo-Json -Depth 10 -Compress
$headers = @{ Authorization = "Bearer $jwt" }
$resp = Invoke-RestMethod -Uri "$Api/api/v1/ingesta" -Method Post -ContentType 'application/json' -Body $payload -Headers $headers
Write-Host ("  [3/4] POST OK: transaccionesRecibidas=" + $resp.transaccionesRecibidas)

# 4) Trigger Hangfire
try {
    Invoke-WebRequest -Uri "$Api/hangfire/recurring/trigger" -Method Post -Body 'jobs[]=anomaly-detection' -ContentType 'application/x-www-form-urlencoded' -UseBasicParsing | Out-Null
    Write-Host '  [4/4] Job de Hangfire disparado.'
} catch {
    Write-Host '  [4/4] Trigger fallo, esperando ciclo automatico (max 5 min).'
}

Write-Host ''
Write-Host '============================================================'
Write-Host ' Espera 10-15 s y refresca el dashboard:'
Write-Host '   http://localhost:5173/alertas'
Write-Host '============================================================'
