# ============================================================================
#  Actualizar el sistema central en PRODUCCION, de forma remota (desde tu PC).
#  Sube tus commits a GitHub y le dice al servidor que se reconstruya.
#  Requisito: tener acceso SSH al servidor (usuario@ip o dominio).
# ============================================================================
$ErrorActionPreference = 'Stop'
Set-Location -Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent)   # raiz del proyecto

$cfg = Join-Path $PSScriptRoot '.deploy.config'
if (Test-Path $cfg) {
    $c = Get-Content $cfg -Raw | ConvertFrom-StringData
    $servidor = $c.SERVIDOR
    $ruta     = $c.RUTA
}
else {
    Write-Host "Configuracion del servidor de produccion (se guarda para la proxima vez):`n" -ForegroundColor Cyan
    $servidor = Read-Host "  Servidor SSH (usuario@ip-o-dominio)"
    $ruta     = Read-Host "  Ruta del proyecto en el servidor (ej. /home/usuario/petrolrios)"
    "SERVIDOR=$servidor`r`nRUTA=$ruta" | Set-Content $cfg
}

Write-Host "`n[1/2] Subiendo tus cambios a GitHub..." -ForegroundColor Cyan
git push

Write-Host "[2/2] Actualizando el central en $servidor (pull + rebuild)..." -ForegroundColor Cyan
ssh $servidor "cd '$ruta' && git pull && docker compose -f docker-compose.prod.yml up -d --build"

Write-Host "`n[OK] Central actualizado en produccion." -ForegroundColor Green
Write-Host "    Los datos (la base) se conservan; solo se reconstruyo y reinicio el central." -ForegroundColor Green
