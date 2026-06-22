# ============================================================================
#  PetrolRios - Instalacion guiada del sistema central
#  No edites esto: ejecuta INSTALAR.bat (doble clic).
# ============================================================================
$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

function Linea($txt, $color = 'Gray') { Write-Host $txt -ForegroundColor $color }
function Get-LanIp {
    # Preferir el adaptador con gateway por defecto = la red real (no el puente de Docker/WSL).
    $ip = Get-NetIPConfiguration |
        Where-Object { $null -ne $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } |
        Select-Object -First 1 -ExpandProperty IPv4Address |
        Select-Object -First 1 -ExpandProperty IPAddress
    if ([string]::IsNullOrWhiteSpace($ip)) {
        # Respaldo: primera IPv4 que no sea loopback, APIPA ni rangos de Docker/WSL.
        $ip = Get-NetIPAddress -AddressFamily IPv4 |
            Where-Object {
                $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' -and
                $_.IPAddress -notlike '172.1[6-9].*' -and $_.IPAddress -notlike '172.2*' -and $_.IPAddress -notlike '172.3*'
            } |
            Select-Object -First 1 -ExpandProperty IPAddress
    }
    if ([string]::IsNullOrWhiteSpace($ip)) { 'localhost' } else { $ip }
}
function New-Secret([int]$n) {
    -join ((48..57) + (65..90) + (97..122) | Get-Random -Count $n | ForEach-Object { [char]$_ })
}

Linea "===================================================" Cyan
Linea "   PetrolRios - Instalacion del sistema central" Cyan
Linea "===================================================`n" Cyan

# 1) Docker instalado y corriendo
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Linea " [X] Docker no esta instalado." Red
    Linea "     Instala 'Docker Desktop' desde https://www.docker.com/products/docker-desktop"
    Linea "     y vuelve a ejecutar este instalador."
    Read-Host "`nEnter para salir"; exit 1
}
docker info *> $null
if ($LASTEXITCODE -ne 0) {
    Linea " [X] Docker esta instalado pero no esta corriendo." Red
    Linea "     Abre 'Docker Desktop', espera a que diga 'Running' y ejecuta esto de nuevo."
    Read-Host "`nEnter para salir"; exit 1
}
Linea " [OK] Docker detectado y corriendo.`n" Green

# 2) Configuracion (.env)
$envPath = Join-Path $PSScriptRoot '.env'
if (Test-Path $envPath) {
    Linea " Ya existe un .env: se conserva tal cual." Yellow
    Linea " Para reconfigurar desde cero, borralo y vuelve a ejecutar.`n" Yellow
}
else {
    Linea " Voy a generar la configuracion. Las contrasenas internas se crean solas.`n"
    Linea " Correo para los avisos del sistema (recuperacion / verificacion de cuenta)."
    Linea " Si todavia no lo tienes, deja ambos vacios y lo pones luego desde Ajustes.`n"
    $emailUser = Read-Host "   Correo (Gmail)"
    $emailPwd  = Read-Host "   App Password del correo"

    $lanip    = Get-LanIp
    $emailHab = if ([string]::IsNullOrWhiteSpace($emailUser)) { 'false' } else { 'true' }

    $contenido = @"
POSTGRES_DB=petrolrios
POSTGRES_USER=petrolrios
POSTGRES_PASSWORD=$(New-Secret 40)
CENTRAL_PORT=8080
JWT_SECRET=$(New-Secret 48)
FRONTEND_URL=http://${lanip}:8080
EMAIL_HABILITADO=$emailHab
EMAIL_HOST=smtp.gmail.com
EMAIL_PUERTO=587
EMAIL_SSL=true
EMAIL_USUARIO=$emailUser
EMAIL_PASSWORD=$emailPwd
EMAIL_REMITENTE=$emailUser
"@
    # UTF-8 sin BOM (docker compose no tolera BOM al inicio del .env)
    [System.IO.File]::WriteAllText($envPath, $contenido)
    Linea "`n [OK] Configuracion guardada en .env  (IP detectada: $lanip)`n" Green
}

# 3) Construir y levantar
Linea " Construyendo y levantando el sistema. La PRIMERA vez tarda unos minutos...`n"
docker compose -f docker-compose.prod.yml up -d --build
if ($LASTEXITCODE -ne 0) {
    Linea "`n [X] Hubo un error al levantar. Revisa el mensaje de arriba." Red
    Read-Host "`nEnter para salir"; exit 1
}

# 4) Resultado
$lanip = Get-LanIp
Linea "`n===================================================" Green
Linea "   LISTO. El sistema central esta corriendo." Green
Linea "===================================================" Green
Linea "   En esta maquina:      http://localhost:8080"
Linea "   Desde otra maquina:   http://${lanip}:8080"
Linea ""
Linea "   Usuario:  admin@petrolrios.com"
Linea "   Clave:    Admin123!   (te pedira cambiarla al entrar)"
Linea ""
Linea "   TIP: en Docker Desktop activa 'Start Docker Desktop when you log in'" Yellow
Linea "        para que el sistema vuelva solo despues de un corte de luz." Yellow
Linea "===================================================`n" Green
Read-Host "Enter para cerrar"
