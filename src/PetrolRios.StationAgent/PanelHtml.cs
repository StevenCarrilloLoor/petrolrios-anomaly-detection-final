namespace PetrolRios.StationAgent;

/// <summary>
/// Página del panel de control local del agente (HTML embebido, sin dependencias).
/// </summary>
internal static class PanelHtml
{
    public const string Pagina = """
<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>PetrolRíos — Station Agent</title>
<style>
  :root{--bg:#0b1220;--card:#111a2e;--border:#1f2a44;--text:#e6ebf5;--muted:#8b96ad;
        --ok:#22c55e;--err:#ef4444;--warn:#eab308;--accent:#3b82f6;}
  *{box-sizing:border-box;margin:0;padding:0}
  body{background:var(--bg);color:var(--text);font-family:'Segoe UI',system-ui,sans-serif;padding:24px}
  .wrap{max-width:980px;margin:0 auto}
  header{display:flex;align-items:center;justify-content:space-between;margin-bottom:20px}
  h1{font-size:20px}
  h1 small{display:block;font-size:11px;color:var(--muted);font-weight:400;letter-spacing:1px;text-transform:uppercase}
  .pill{display:inline-flex;align-items:center;gap:8px;border:1px solid var(--border);
        border-radius:999px;padding:6px 14px;font-size:13px;background:var(--card)}
  .dot{width:9px;height:9px;border-radius:50%;background:var(--ok);box-shadow:0 0 8px var(--ok)}
  .dot.err{background:var(--err);box-shadow:0 0 8px var(--err)}
  .dot.warn{background:var(--warn);box-shadow:0 0 8px var(--warn)}
  .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:14px;margin-bottom:14px}
  .card{background:var(--card);border:1px solid var(--border);border-radius:12px;padding:16px}
  .card h3{font-size:12px;color:var(--muted);font-weight:600;text-transform:uppercase;letter-spacing:.5px;margin-bottom:8px}
  .big{font-size:22px;font-weight:700}
  .sub{font-size:12px;color:var(--muted);margin-top:4px}
  .row{display:flex;justify-content:space-between;font-size:13px;padding:3px 0}
  .row span:first-child{color:var(--muted)}
  .mono{font-family:Consolas,monospace;font-size:12px}
  .acciones{display:flex;flex-wrap:wrap;gap:10px;margin:18px 0}
  button{background:var(--accent);border:none;color:#fff;font-weight:600;font-size:13px;
         padding:10px 18px;border-radius:8px;cursor:pointer}
  button:hover{filter:brightness(1.15)}
  button.sec{background:transparent;border:1px solid var(--border);color:var(--text)}
  button:disabled{opacity:.5;cursor:wait}
  .switch{display:flex;align-items:center;gap:10px;background:var(--card);border:1px solid var(--border);
          border-radius:8px;padding:8px 14px;font-size:13px}
  .toggle{position:relative;width:44px;height:24px;border-radius:999px;background:#374151;cursor:pointer;transition:.2s}
  .toggle.on{background:var(--ok)}
  .toggle::after{content:'';position:absolute;top:2px;left:2px;width:20px;height:20px;border-radius:50%;
                 background:#fff;transition:.2s}
  .toggle.on::after{left:22px}
  #log{background:#0a0f1c;border:1px solid var(--border);border-radius:12px;padding:14px;
       max-height:260px;overflow-y:auto;font-family:Consolas,monospace;font-size:12px;line-height:1.7}
  .ev-ok{color:var(--ok)} .ev-err{color:var(--err)} .ev-info{color:var(--muted)}
  .resultado{margin-top:8px;font-size:13px;padding:10px 12px;border-radius:8px;display:none}
  .resultado.ok{display:block;background:rgba(34,197,94,.1);color:var(--ok);border:1px solid rgba(34,197,94,.3)}
  .resultado.err{display:block;background:rgba(239,68,68,.1);color:var(--err);border:1px solid rgba(239,68,68,.3)}
  footer{margin-top:18px;font-size:11px;color:var(--muted);text-align:center}
</style>
</head>
<body>
<div class="wrap">
  <header>
    <h1>⛽ Station Agent <span id="estacion"></span><small>PetrolRíos · Sistema de Detección de Anomalías</small></h1>
    <div class="pill"><span class="dot" id="dot-modo"></span><span id="txt-modo">—</span></div>
  </header>

  <div class="grid">
    <div class="card">
      <h3>Último ciclo</h3>
      <div class="big" id="ultimo-ciclo">—</div>
      <div class="sub" id="ultimo-resultado">—</div>
    </div>
    <div class="card">
      <h3>Transacciones enviadas</h3>
      <div class="big" id="total-enviadas">0</div>
      <div class="sub"><span id="ciclos">0</span> ciclos ejecutados</div>
    </div>
    <div class="card">
      <h3>Pendientes (store-and-forward)</h3>
      <div class="big" id="pendientes">0</div>
      <div class="sub">lotes guardados sin enviar</div>
    </div>
    <div class="card">
      <h3>Conexión con el servidor</h3>
      <div class="big" id="latencia">—</div>
      <div class="sub" id="ultima-conexion">sin datos</div>
    </div>
  </div>

  <div class="grid">
    <div class="card" style="grid-column:1/-1">
      <h3>Configuración activa</h3>
      <div class="row"><span>Estación</span><span class="mono" id="cfg-estacion">—</span></div>
      <div class="row"><span>Servidor central</span><span class="mono" id="cfg-servidor">—</span></div>
      <div class="row"><span>Base Firebird</span><span class="mono" id="cfg-firebird">—</span></div>
      <div class="row"><span>Intervalo automático</span><span class="mono" id="cfg-intervalo">—</span></div>
      <div class="row"><span>Marca de agua (watermark)</span><span class="mono" id="cfg-watermark">—</span></div>
      <div class="row"><span>Agente iniciado</span><span class="mono" id="cfg-uptime">—</span></div>
    </div>
  </div>

  <div class="acciones">
    <div class="switch">
      <span>Sincronización automática</span>
      <div class="toggle" id="toggle-modo" onclick="cambiarModo()"></div>
    </div>
    <button id="btn-sync" onclick="sincronizar()">⟳ Sincronizar ahora</button>
    <button class="sec" onclick="probar('firebird')">Probar conexión Firebird</button>
    <button class="sec" onclick="probar('servidor')">Probar conexión al servidor</button>
  </div>
  <div class="resultado" id="resultado"></div>

  <div class="card" style="margin-top:14px">
    <h3>Actividad reciente</h3>
    <div id="log">cargando…</div>
  </div>

  <footer>Panel local del agente · solo accesible desde esta máquina (localhost) · PetrolRíos S.A.</footer>
</div>

<script>
let modoAuto = true;

function fmtFecha(iso){
  if(!iso) return '—';
  return new Date(iso).toLocaleString('es-EC',{dateStyle:'short',timeStyle:'medium'});
}
function fmtUptime(s){
  if(s<60) return Math.round(s)+' s';
  if(s<3600) return Math.floor(s/60)+' min';
  return Math.floor(s/3600)+' h '+Math.floor(s%3600/60)+' min';
}

async function refrescar(){
  try{
    const r = await fetch('/api/estado');
    const e = await r.json();
    modoAuto = e.modoAutomatico;

    document.getElementById('estacion').textContent = '· ' + e.estacion;
    document.getElementById('dot-modo').className = 'dot' + (modoAuto ? '' : ' warn');
    document.getElementById('txt-modo').textContent = modoAuto ? 'Automático' : 'Manual';
    document.getElementById('toggle-modo').className = 'toggle' + (modoAuto ? ' on' : '');

    document.getElementById('ultimo-ciclo').textContent = e.ultimoCiclo ? fmtFecha(e.ultimoCiclo) : '—';
    const res = document.getElementById('ultimo-resultado');
    res.textContent = e.ultimoResultado;
    res.style.color = e.ultimoCicloExitoso ? '' : 'var(--err)';

    document.getElementById('total-enviadas').textContent = e.totalTransaccionesEnviadas;
    document.getElementById('ciclos').textContent = e.ciclosEjecutados;
    document.getElementById('pendientes').textContent = e.lotesPendientes;
    document.getElementById('pendientes').style.color = e.lotesPendientes > 0 ? 'var(--warn)' : '';

    document.getElementById('latencia').textContent =
      e.ultimaLatenciaServidorMs != null ? e.ultimaLatenciaServidorMs + ' ms' : '—';
    document.getElementById('ultima-conexion').textContent =
      e.ultimaConexionServidor ? 'última conexión: ' + fmtFecha(e.ultimaConexionServidor)
      : (e.ultimaDesconexionServidor ? 'última desconexión: ' + fmtFecha(e.ultimaDesconexionServidor) : 'sin datos');

    document.getElementById('cfg-estacion').textContent = e.estacion;
    document.getElementById('cfg-servidor').textContent = e.servidor;
    document.getElementById('cfg-firebird').textContent = e.firebird;
    document.getElementById('cfg-intervalo').textContent = 'cada ' + e.intervaloSegundos + ' s';
    document.getElementById('cfg-watermark').textContent = fmtFecha(e.watermark);
    document.getElementById('cfg-uptime').textContent = fmtFecha(e.inicioAgente) + ' (hace ' + fmtUptime(e.uptimeSegundos) + ')';

    document.getElementById('log').innerHTML = (e.eventos || []).map(ev =>
      `<div class="ev-${ev.nivel === 'OK' ? 'ok' : ev.nivel === 'ERROR' ? 'err' : 'info'}">` +
      `[${new Date(ev.fecha).toLocaleTimeString('es-EC')}] ${ev.nivel.padEnd(5)} ${ev.mensaje}</div>`
    ).join('') || 'sin eventos';
  }catch(err){
    document.getElementById('txt-modo').textContent = 'Agente sin respuesta';
    document.getElementById('dot-modo').className = 'dot err';
  }
}

function mostrarResultado(ok, texto){
  const el = document.getElementById('resultado');
  el.className = 'resultado ' + (ok ? 'ok' : 'err');
  el.textContent = (ok ? '✔ ' : '✘ ') + texto;
}

async function sincronizar(){
  const btn = document.getElementById('btn-sync');
  btn.disabled = true; btn.textContent = '⟳ Sincronizando…';
  try{
    const r = await fetch('/api/sincronizar', {method:'POST'});
    const j = await r.json();
    mostrarResultado(j.ok, j.resultado);
  }catch(e){ mostrarResultado(false, 'No se pudo contactar al agente'); }
  btn.disabled = false; btn.textContent = '⟳ Sincronizar ahora';
  refrescar();
}

async function cambiarModo(){
  try{
    const r = await fetch('/api/modo', {method:'POST',
      headers:{'Content-Type':'application/json'},
      body: JSON.stringify({automatico: !modoAuto})});
    await r.json();
  }catch(e){}
  refrescar();
}

async function probar(cual){
  mostrarResultado(true, 'Probando conexión…');
  try{
    const r = await fetch('/api/probar-' + cual, {method:'POST'});
    const j = await r.json();
    mostrarResultado(j.ok, j.mensaje);
  }catch(e){ mostrarResultado(false, 'No se pudo contactar al agente'); }
  refrescar();
}

refrescar();
setInterval(refrescar, 5000);
</script>
</body>
</html>
""";
}
