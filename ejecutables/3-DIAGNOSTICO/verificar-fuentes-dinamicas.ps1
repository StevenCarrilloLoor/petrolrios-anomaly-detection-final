# ============================================================
# RESUMEN (que hace este script):
# VERIFICA LAS FUENTES DINAMICAS
# Consulta el agente (5180) y el central (5170) y reporta el estado de sincronizacion de las fuentes configurables.
# ============================================================
param(
    [string]$ServerUrl = "http://localhost:5170",
    [string]$AgentUrl = "http://localhost:5180",
    [string]$Email = ""
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== DOBLE CHECK DE FUENTES DINAMICAS ===" -ForegroundColor Cyan

try {
    $estadoAgente = Invoke-RestMethod "$AgentUrl/api/estado"
    Write-Host ""
    Write-Host "AGENTE LOCAL ($($estadoAgente.estacion))" -ForegroundColor Yellow
    if (-not $estadoAgente.fuentesCentrales -or $estadoAgente.fuentesCentrales.Count -eq 0) {
        Write-Host "  El agente no ha recibido fuentes centrales activas."
    }
    else {
        $estadoAgente.fuentesCentrales |
            Select-Object nombre, tabla, columnaWatermark, estado, filasLeidas, filasEnviadas, ultimoError |
            Format-Table -AutoSize -Wrap
    }
}
catch {
    Write-Host "No se pudo consultar el panel del agente: $($_.Exception.Message)" -ForegroundColor Red
}

if ([string]::IsNullOrWhiteSpace($Email)) {
    $Email = Read-Host "Correo de Administrador o Supervisor para consultar el central"
}
$segura = Read-Host "Contrasena" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
}

try {
    $login = Invoke-RestMethod -Method Post -Uri "$ServerUrl/api/v1/auth/login" `
        -ContentType "application/json" `
        -Body (@{ email = $Email; password = $password } | ConvertTo-Json)
    $headers = @{ Authorization = "Bearer $($login.token)" }
    $fuentes = Invoke-RestMethod -Method Get -Uri "$ServerUrl/api/v1/fuentes-datos" -Headers $headers

    Write-Host ""
    Write-Host "SISTEMA CENTRAL" -ForegroundColor Yellow
    foreach ($fuente in $fuentes) {
        Write-Host ""
        Write-Host "$($fuente.nombre) [$($fuente.tabla)] cursor=$($fuente.columnaWatermark)" -ForegroundColor Cyan
        if (-not $fuente.sincronizaciones -or $fuente.sincronizaciones.Count -eq 0) {
            Write-Host "  Sin reportes de estaciones." -ForegroundColor DarkYellow
            continue
        }
        $fuente.sincronizaciones |
            Select-Object estacionCodigo, agenteEnLinea, estado, configuracionActualizada,
                filasLeidas, filasEnviadas, totalFilasEnviadas, ultimoReporte, ultimoError |
            Format-Table -AutoSize -Wrap
    }
}
catch {
    Write-Host "No se pudo consultar el central: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    $password = $null
}
