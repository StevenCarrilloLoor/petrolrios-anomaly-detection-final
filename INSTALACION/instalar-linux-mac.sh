#!/usr/bin/env bash
# ============================================================================
#  PetrolRios - Instalacion del sistema central (Linux y macOS)
#  Uso:  chmod +x instalar-linux-mac.sh   &&   ./instalar-linux-mac.sh
# ============================================================================
set -euo pipefail
cd "$(dirname "$0")/.."   # ir a la raiz del proyecto (donde estan los compose)

echo "==================================================="
echo "   PetrolRios - Instalacion del sistema central"
echo "==================================================="
echo

# 1) Docker instalado y corriendo
if ! command -v docker >/dev/null 2>&1; then
  echo " [X] Docker no esta instalado."
  echo "     Linux: https://docs.docker.com/engine/install/"
  echo "     macOS: Docker Desktop -> https://www.docker.com/products/docker-desktop"
  exit 1
fi
if ! docker info >/dev/null 2>&1; then
  echo " [X] Docker esta instalado pero no responde."
  echo "     Linux: sudo systemctl start docker   |   macOS: abre Docker Desktop"
  exit 1
fi
echo " [OK] Docker detectado y corriendo."
echo

# Helpers
gen_secret() { LC_ALL=C tr -dc 'A-Za-z0-9' </dev/urandom | head -c "${1:-40}"; }
lan_ip() {
  local ip=""
  if command -v ip >/dev/null 2>&1; then
    ip=$(ip route get 1.1.1.1 2>/dev/null | awk '{for(i=1;i<=NF;i++) if($i=="src"){print $(i+1); exit}}')
  fi
  if [ -z "$ip" ] && command -v ipconfig >/dev/null 2>&1; then   # macOS
    ip=$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null || true)
  fi
  if [ -z "$ip" ] && command -v hostname >/dev/null 2>&1; then
    ip=$(hostname -I 2>/dev/null | awk '{print $1}')
  fi
  [ -z "$ip" ] && ip="localhost"
  echo "$ip"
}

# 2) Configuracion (.env)
if [ -f .env ]; then
  echo " Ya existe un .env: se conserva. Para reconfigurar, borralo y reejecuta."
  echo
else
  echo " Voy a generar la configuracion. Las contrasenas internas se crean solas."
  echo " Correo para los avisos (recuperacion/verificacion). Si no lo tienes, deja vacio."
  read -rp "   Correo (Gmail): " EMAIL_USER
  read -rp "   App Password del correo: " EMAIL_PWD
  LANIP="$(lan_ip)"
  EMAIL_HAB="true"; [ -z "$EMAIL_USER" ] && EMAIL_HAB="false"

  cat > .env <<EOF
POSTGRES_DB=petrolrios
POSTGRES_USER=petrolrios
POSTGRES_PASSWORD=$(gen_secret 40)
CENTRAL_PORT=8080
JWT_SECRET=$(gen_secret 48)
FRONTEND_URL=http://$LANIP:8080
EMAIL_HABILITADO=$EMAIL_HAB
EMAIL_HOST=smtp.gmail.com
EMAIL_PUERTO=587
EMAIL_SSL=true
EMAIL_USUARIO=$EMAIL_USER
EMAIL_PASSWORD=$EMAIL_PWD
EMAIL_REMITENTE=$EMAIL_USER
EOF
  echo
  echo " [OK] Configuracion guardada en .env  (IP detectada: $LANIP)"
  echo
fi

# 3) Construir y levantar
echo " Construyendo y levantando el sistema. La PRIMERA vez tarda unos minutos..."
echo
docker compose -f docker-compose.prod.yml up -d --build

# 4) Resultado
LANIP="$(lan_ip)"
echo
echo "==================================================="
echo "   LISTO. El sistema central esta corriendo."
echo "==================================================="
echo "   En esta maquina:     http://localhost:8080"
echo "   Desde otra maquina:  http://$LANIP:8080"
echo
echo "   Usuario:  admin@petrolrios.com"
echo "   Clave:    Admin123!   (te pedira cambiarla al entrar)"
echo
echo "   Para que vuelva tras un reinicio, Docker ya reinicia los contenedores."
echo "   En Linux, habilita Docker al arranque:  sudo systemctl enable docker"
echo "==================================================="
