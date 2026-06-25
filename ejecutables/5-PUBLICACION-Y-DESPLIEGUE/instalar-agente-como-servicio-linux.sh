#!/usr/bin/env bash
# ============================================================
#  Instala el Agente PetrolRios como servicio systemd en Linux
#  (arranca automaticamente con el sistema). Ejecutar con sudo
#  DENTRO de la carpeta del agente:   sudo ./instalar_agente_servicio.sh
# ============================================================
set -euo pipefail

NOMBRE="petrolrios-agente"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
EXE="$DIR/PetrolRios.StationAgent"
UNIT="/etc/systemd/system/${NOMBRE}.service"

if [ ! -f "$EXE" ]; then
  echo "ERROR: no se encontro PetrolRios.StationAgent en esta carpeta."
  echo "Copie este script DENTRO de la carpeta del agente publicado y reintente."
  exit 1
fi

if [ "$(id -u)" -ne 0 ]; then
  echo "ERROR: ejecute con sudo:   sudo ./instalar_agente_servicio.sh"
  exit 1
fi

# El binario debe ser ejecutable (al venir de Windows pierde el bit +x)
chmod +x "$EXE"

# Usuario que invoco sudo (para que el servicio no corra como root sin necesidad)
USUARIO="${SUDO_USER:-root}"

echo "Creando unidad systemd en $UNIT ..."
cat > "$UNIT" <<EOF
[Unit]
Description=Agente de estacion PetrolRios (extrae de Firebird y envia al servidor central)
After=network-online.target firebird.service
Wants=network-online.target

[Service]
Type=simple
ExecStart=$EXE
WorkingDirectory=$DIR
Restart=on-failure
RestartSec=10
User=$USUARIO

[Install]
WantedBy=multi-user.target
EOF

echo "Recargando systemd y habilitando el servicio..."
systemctl daemon-reload
systemctl enable "$NOMBRE"
systemctl restart "$NOMBRE"

echo
echo "============================================================"
echo " Listo. El agente quedo como servicio y arrancara con el sistema."
echo " Panel de control:  http://localhost:5180"
echo
echo " Ver estado:    sudo systemctl status $NOMBRE"
echo " Ver logs:      sudo journalctl -u $NOMBRE -f"
echo " Detener:       sudo systemctl stop $NOMBRE"
echo " Desinstalar:   sudo systemctl disable --now $NOMBRE && sudo rm $UNIT && sudo systemctl daemon-reload"
echo "============================================================"
