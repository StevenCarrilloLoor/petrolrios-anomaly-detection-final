#!/usr/bin/env bash
# ============================================================================
#  Actualizar el sistema central en PRODUCCION, de forma remota (desde tu PC).
#  Sube tus commits a GitHub y le dice al servidor que se reconstruya.
#  Requisito: acceso SSH al servidor (usuario@ip o dominio).
#  Uso:  chmod +x actualizar-central.sh && ./actualizar-central.sh
# ============================================================================
set -euo pipefail
cd "$(dirname "$0")/../.."   # raiz del proyecto
CFG="$(dirname "$0")/.deploy.config"

if [ -f "$CFG" ]; then
  # shellcheck disable=SC1090
  . "$CFG"
else
  echo "Configuracion del servidor de produccion (se guarda para la proxima vez):"
  read -rp "  Servidor SSH (usuario@ip-o-dominio): " SERVIDOR
  read -rp "  Ruta del proyecto en el servidor (ej. /home/usuario/petrolrios): " RUTA
  printf 'SERVIDOR=%s\nRUTA=%s\n' "$SERVIDOR" "$RUTA" > "$CFG"
fi

echo
echo "[1/2] Subiendo tus cambios a GitHub..."
git push

echo "[2/2] Actualizando el central en $SERVIDOR (pull + rebuild)..."
ssh "$SERVIDOR" "cd '$RUTA' && git pull && docker compose -f docker-compose.prod.yml up -d --build"

echo
echo "[OK] Central actualizado en produccion."
echo "    Los datos (la base) se conservan; solo se reconstruyo y reinicio el central."
