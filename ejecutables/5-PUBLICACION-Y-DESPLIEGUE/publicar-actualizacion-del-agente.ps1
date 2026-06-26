# ============================================================================
#  PetrolRios - Publica una actualizacion del AGENTE para que las estaciones
#  la apliquen con un clic (control de versiones).
#  No edites esto: ejecuta publicar-actualizacion-del-agente.bat (doble clic).
# ============================================================================
param(
    [string]$Notas = "",
    [switch]$Obligatoria
)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent   # raiz del proyecto
Set-Location -Path $raiz

function Linea($t, $c = 'Gray') { Write-Host $t -ForegroundColor $c }

Linea "===================================================" Cyan
Linea "   Publicar actualizacion del Agente PetrolRios" Cyan
Linea "===================================================`n" Cyan

# 1) Version (fuente unica: Directory.Build.props)
$props = Get-Content (Join-Path $raiz 'Directory.Build.props') -Raw
$version = [regex]::Match($props, '<Version>([^<]+)</Version>').Groups[1].Value
if ([string]::IsNullOrWhiteSpace($version)) { Linea " [X] No pude leer <Version> de Directory.Build.props." Red; exit 1 }
Linea " Version a publicar: $version`n"

# 2) Localizar el exe del agente ya publicado
$candidatos = @(
    (Join-Path $raiz 'dist\agente-windows\PetrolRios.StationAgent.exe'),
    (Join-Path $raiz 'dist\PetrolRios-Agente\PetrolRios.StationAgent.exe')
)
$exe = $candidatos | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $exe) {
    Linea " [X] No encontre el exe del agente en dist\." Red
    Linea "     Corre primero  publicar-solo-el-agente-multiplataforma.bat  (genera dist\agente-windows)."
    exit 1
}
Linea " Ejecutable: $exe"

# 3) SHA256 (verificacion de integridad de la descarga)
$sha = (Get-FileHash $exe -Algorithm SHA256).Hash.ToLower()
Linea " SHA256: $sha`n"

# 4) Carpetas que monta docker-compose.prod.yml (las crea si faltan)
$descargas = Join-Path $raiz 'central-descargas'
$config    = Join-Path $raiz 'central-config'
New-Item -ItemType Directory -Force -Path $descargas, $config | Out-Null

# 5) Copiar el exe a la carpeta de descargas del central
Copy-Item $exe (Join-Path $descargas 'PetrolRios.StationAgent.exe') -Force

# 6) Escribir el manifiesto. La URL es RELATIVA: el central la vuelve absoluta con su propio host,
#    asi el manifiesto no lleva una IP fija y sirve en cualquier red.
if ([string]::IsNullOrWhiteSpace($Notas)) { $Notas = "Version $version del agente PetrolRios." }
$manifiesto = [ordered]@{
    version     = $version
    url         = '/descargas/PetrolRios.StationAgent.exe'
    sha256      = $sha
    notas       = $Notas
    obligatoria = [bool]$Obligatoria
}
$destinoJson = Join-Path $config 'agente-version.json'
$manifiesto | ConvertTo-Json | Set-Content -Path $destinoJson -Encoding UTF8

Linea "===================================================" Green
Linea "   LISTO. Actualizacion $version publicada." Green
Linea "===================================================" Green
Linea "   central-config\agente-version.json   (manifiesto)"
Linea "   central-descargas\PetrolRios.StationAgent.exe"
Linea ""
Linea " Si el central corre con Docker, reinicia para montar las carpetas:" Yellow
Linea "   docker compose -f docker-compose.prod.yml up -d" Yellow
Linea ""
Linea " En cada estacion: el panel del agente mostrara la actualizacion en su"
Linea " proximo ciclo; pulsa 'Aplicar actualizacion' para instalarla."
Linea "===================================================`n" Green
