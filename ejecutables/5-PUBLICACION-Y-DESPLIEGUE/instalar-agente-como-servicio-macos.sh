#!/usr/bin/env bash
# ============================================================
#  Instala el Agente PetrolRios como servicio launchd en macOS
#  (arranca automaticamente al iniciar sesion). Ejecutar DENTRO
#  de la carpeta del agente:   ./instalar_agente_servicio_macos.sh
# ============================================================
set -euo pipefail

ETIQUETA="com.petrolrios.agente"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EXE="$DIR/PetrolRios.StationAgent"
PLIST="$HOME/Library/LaunchAgents/${ETIQUETA}.plist"

if [ ! -f "$EXE" ]; then
  echo "ERROR: no se encontro PetrolRios.StationAgent en esta carpeta."
  echo "Copie este script DENTRO de la carpeta del agente publicado y reintente."
  exit 1
fi

# El binario debe ser ejecutable (al venir de Windows pierde el bit +x)
chmod +x "$EXE"
# macOS marca en cuarentena los binarios descargados; lo quitamos para que abra
xattr -d com.apple.quarantine "$EXE" 2>/dev/null || true

mkdir -p "$HOME/Library/LaunchAgents"

echo "Creando agente launchd en $PLIST ..."
cat > "$PLIST" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>${ETIQUETA}</string>
    <key>ProgramArguments</key>
    <array>
        <string>${EXE}</string>
    </array>
    <key>WorkingDirectory</key>
    <string>${DIR}</string>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>${DIR}/logs/launchd.out.log</string>
    <key>StandardErrorPath</key>
    <string>${DIR}/logs/launchd.err.log</string>
</dict>
</plist>
EOF

mkdir -p "$DIR/logs"

echo "Cargando el servicio..."
launchctl unload "$PLIST" 2>/dev/null || true
launchctl load "$PLIST"

echo
echo "============================================================"
echo " Listo. El agente quedo como servicio y arrancara al iniciar sesion."
echo " Panel de control:  http://localhost:5180"
echo
echo " Ver si corre:  launchctl list | grep petrolrios"
echo " Detener:       launchctl unload \"$PLIST\""
echo " Desinstalar:   launchctl unload \"$PLIST\" && rm \"$PLIST\""
echo "============================================================"
