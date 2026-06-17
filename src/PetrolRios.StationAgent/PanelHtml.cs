namespace PetrolRios.StationAgent;

/// <summary>
/// Página del panel de control local del agente (HTML embebido, sin dependencias).
/// Incluye monitoreo en vivo y un formulario de configuración con opciones
/// avanzadas de conexión (Firebird y servidor central) editables en campo.
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
  .wrap{max-width:1000px;margin:0 auto}
  header{display:flex;align-items:center;justify-content:space-between;margin-bottom:18px;flex-wrap:wrap;gap:10px}
  h1{font-size:20px}
  h1 small{display:block;font-size:11px;color:var(--muted);font-weight:400;letter-spacing:1px;text-transform:uppercase}
  .pill{display:inline-flex;align-items:center;gap:8px;border:1px solid var(--border);
        border-radius:999px;padding:6px 14px;font-size:13px;background:var(--card)}
  .dot{width:9px;height:9px;border-radius:50%;background:var(--ok);box-shadow:0 0 8px var(--ok)}
  .dot.err{background:var(--err);box-shadow:0 0 8px var(--err)}
  .dot.warn{background:var(--warn);box-shadow:0 0 8px var(--warn)}
  .tabs{display:flex;gap:6px;margin-bottom:16px;border-bottom:1px solid var(--border)}
  .tab{padding:10px 18px;cursor:pointer;font-size:14px;color:var(--muted);border-bottom:2px solid transparent}
  .tab.active{color:var(--text);border-bottom-color:var(--accent);font-weight:600}
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
  /* Formulario de configuración */
  .form-section{margin-bottom:20px}
  .form-section h2{font-size:14px;margin-bottom:10px;color:var(--accent);
                   border-bottom:1px solid var(--border);padding-bottom:6px}
  .fields{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:12px}
  .field{display:flex;flex-direction:column;gap:4px}
  .field label{font-size:12px;color:var(--muted)}
  .field input,.field select{background:#0a0f1c;border:1px solid var(--border);color:var(--text);
        border-radius:7px;padding:9px 11px;font-size:13px;font-family:inherit}
  .field input:focus,.field select:focus{outline:none;border-color:var(--accent)}
  .field .hint{font-size:10px;color:var(--muted)}
  .banner{background:rgba(234,179,8,.1);border:1px solid rgba(234,179,8,.35);color:var(--warn);
          border-radius:10px;padding:12px 16px;margin-bottom:16px;font-size:13px;display:none}
  .banner.show{display:block}
  .banner-update{background:rgba(59,130,246,.12);border-color:rgba(59,130,246,.4);color:#bfdbfe;
                 display:none;align-items:center;justify-content:space-between;gap:14px}
  .banner-update.show{display:flex}
  .hidden{display:none}
  /* Login overlay */
  #overlay-login{position:fixed;inset:0;background:rgba(8,12,22,.96);display:none;
                 align-items:center;justify-content:center;z-index:50;padding:20px}
  #overlay-login.show{display:flex}
  .login-card{background:var(--card);border:1px solid var(--border);border-radius:14px;
              padding:28px;width:100%;max-width:380px}
  .login-card h2{font-size:18px;margin-bottom:6px}
  .login-card p{font-size:12px;color:var(--muted);margin-bottom:18px}
  .login-card label{display:block;font-size:12px;color:var(--muted);margin:10px 0 4px}
  .login-card input{width:100%;background:#0a0f1c;border:1px solid var(--border);border-radius:8px;
                    color:var(--text);padding:10px 12px;font-size:14px}
  .login-card button{width:100%;margin-top:16px}
  .sesion-pill{display:inline-flex;align-items:center;gap:8px;font-size:12px;color:var(--muted)}
  .link{background:none;border:none;color:var(--accent);cursor:pointer;font-size:12px;padding:0}
</style>
</head>
<body>

<!-- Pantalla de inicio de sesión del panel -->
<div id="overlay-login">
  <div class="login-card">
    <h2>🔒 Acceso al agente</h2>
    <p>Inicie sesión con su cuenta de <b>Administrador</b> o <b>Supervisor</b> de PetrolRíos.
       Las credenciales se verifican contra el servidor central.</p>
    <label>Usuario (email)</label>
    <input id="login-usuario" type="text" autocomplete="username" placeholder="usuario@petrolrios.com">
    <label>Contraseña</label>
    <input id="login-password" type="password" autocomplete="current-password" onkeydown="if(event.key==='Enter')hacerLogin()">
    <button onclick="hacerLogin()">Iniciar sesión</button>
    <div class="resultado" id="resultado-login"></div>
  </div>
</div>

<div class="wrap">
  <header>
    <h1>⛽ Station Agent <span id="estacion"></span><small>PetrolRíos · Sistema de Detección de Anomalías</small></h1>
    <div style="display:flex;align-items:center;gap:14px">
      <span class="sesion-pill" id="sesion-info"></span>
      <div class="pill"><span class="dot" id="dot-modo"></span><span id="txt-modo">—</span></div>
    </div>
  </header>

  <div class="banner" id="banner-setup">
    ⚙️ Este agente aún no está configurado. Vaya a la pestaña <b>Configuración</b>, ingrese el
    <b>nombre de la estación</b> y los datos de conexión, pruebe y guarde para empezar a enviar datos.
  </div>

  <div class="banner banner-update" id="banner-update">
    <div>⬆️ <b>Actualización disponible</b> — versión <span id="upd-version">—</span>.
      <span id="upd-notas" class="sub"></span></div>
    <button id="btn-aplicar-upd" onclick="aplicarActualizacion()">Aplicar y reiniciar</button>
  </div>

  <div class="tabs">
    <div class="tab active" id="tab-monitoreo" onclick="cambiarTab('monitoreo')">Monitoreo</div>
    <div class="tab" id="tab-config" onclick="cambiarTab('config')">Configuración</div>
  </div>

  <!-- ════════ MONITOREO ════════ -->
  <div id="vista-monitoreo">
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
        <div class="row"><span>Nombre</span><span class="mono" id="cfg-nombre">—</span></div>
        <div class="row"><span>Servidor central</span><span class="mono" id="cfg-servidor">—</span></div>
        <div class="row"><span>Base Firebird</span><span class="mono" id="cfg-firebird">—</span></div>
        <div class="row"><span>Intervalo automático</span><span class="mono" id="cfg-intervalo">—</span></div>
        <div class="row"><span>Marca de agua (watermark)</span><span class="mono" id="cfg-watermark">—</span></div>
        <div class="row"><span>Agente iniciado</span><span class="mono" id="cfg-uptime">—</span></div>
        <div class="row"><span>Versión del agente</span><span class="mono" id="cfg-version">—</span></div>
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
      <button class="sec" id="btn-buscar-upd" onclick="buscarActualizacion()">Buscar actualización</button>
    </div>
    <div class="resultado" id="resultado"></div>

    <div class="card" style="margin-top:14px">
      <h3>Actividad reciente</h3>
      <div id="log">cargando…</div>
    </div>
  </div>

  <!-- ════════ CONFIGURACIÓN ════════ -->
  <div id="vista-config" class="hidden">
    <div class="card">
      <div class="form-section">
        <h2>Identidad de la estación</h2>
        <div class="fields">
          <div class="field">
            <label>Código de estación *</label>
            <input id="f-codigo" placeholder="EST-001">
            <span class="hint">Identificador único (se enviará en mayúsculas)</span>
          </div>
          <div class="field">
            <label>Nombre de la estación *</label>
            <input id="f-nombre" placeholder="Ej.: Estación Santo Domingo Centro">
            <span class="hint">Se mostrará en el panel central de Conexiones</span>
          </div>
          <div class="field">
            <label>Zona</label>
            <input id="f-zona" placeholder="Ej.: Centro">
          </div>
        </div>
      </div>

      <div class="form-section">
        <h2>Servidor central</h2>
        <div class="fields">
          <div class="field">
            <label>URL del servidor *</label>
            <input id="f-serverurl" placeholder="http://192.168.1.10:5170">
            <span class="hint">Donde corre la API principal (otra computadora)</span>
          </div>
          <div class="field">
            <label>Usuario (email)</label>
            <input id="f-email" placeholder="agent-est-001@petrolrios.com">
          </div>
          <div class="field">
            <label>Contraseña</label>
            <input id="f-password" type="password" placeholder="(sin cambios)">
            <span class="hint">Déjelo vacío para conservar la actual</span>
          </div>
          <div class="field">
            <label>Timeout (segundos)</label>
            <input id="f-timeout" type="number" min="5" max="120">
          </div>
        </div>
      </div>

      <div class="form-section">
        <h2>Base de datos Firebird local (Contaplus)</h2>
        <div class="fields">
          <div class="field">
            <label>Host / IP</label>
            <input id="f-fbhost" placeholder="localhost">
          </div>
          <div class="field">
            <label>Puerto</label>
            <input id="f-fbport" type="number" placeholder="3050">
          </div>
          <div class="field" style="grid-column:1/-1">
            <label>Ruta de la base (CONTAC.FDB)</label>
            <input id="f-fbdatabase" placeholder="C:\CONTAC\CONTAC.FDB">
          </div>
          <div class="field">
            <label>Usuario</label>
            <input id="f-fbuser" placeholder="SYSDBA">
          </div>
          <div class="field">
            <label>Contraseña</label>
            <input id="f-fbpassword" type="password" placeholder="(sin cambios)">
          </div>
          <div class="field">
            <label>Charset</label>
            <select id="f-fbcharset">
              <option>NONE</option><option>UTF8</option><option>ISO8859_1</option><option>WIN1252</option>
            </select>
          </div>
          <div class="field">
            <label>Dialect</label>
            <select id="f-fbdialect"><option>3</option><option>1</option></select>
          </div>
          <div class="field">
            <label>WireCrypt</label>
            <select id="f-fbwirecrypt">
              <option value="Disabled">Disabled (Firebird 2.5)</option>
              <option value="Enabled">Enabled (Firebird 3+)</option>
            </select>
            <span class="hint">Use Disabled para FB 2.5 con Legacy_Auth</span>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h2>Operación</h2>
        <div class="fields">
          <div class="field">
            <label>Intervalo de sincronización (segundos)</label>
            <input id="f-intervalo" type="number" min="5" max="3600">
            <span class="hint">Cada cuánto extrae y envía (default 30 s)</span>
          </div>
          <div class="field">
            <label>Iniciar en modo automático</label>
            <select id="f-auto"><option value="true">Sí — sincroniza solo</option><option value="false">No — manual</option></select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h2>Actualizaciones</h2>
        <div class="fields">
          <div class="field" style="grid-column:1/-1">
            <label>URL del feed de actualización</label>
            <input id="f-updateurl" type="text" placeholder="(vacío = usar el servidor central)">
            <span class="hint">Déjelo vacío para usar el servidor central. Puede apuntar a un JSON en GitHub si no hay servidor disponible.</span>
          </div>
          <div class="field" style="grid-column:1/-1">
            <label>URL de respaldo (opcional)</label>
            <input id="f-updateurl2" type="text" placeholder="https://raw.githubusercontent.com/.../agente-version.json">
            <span class="hint">Se usa si la primaria falla.</span>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h2>Seguridad del panel (acceso offline)</h2>
        <div class="fields">
          <div class="field">
            <label>Usuario local de respaldo</label>
            <input id="f-localuser" type="text" placeholder="admin-local">
            <span class="hint">Para entrar al panel si el servidor central no está disponible.</span>
          </div>
          <div class="field">
            <label>Contraseña local de respaldo</label>
            <input id="f-localpass" type="password" placeholder="(sin cambios)" autocomplete="new-password">
            <span class="hint">Déjela vacía para conservar la actual. Se guarda cifrada (PBKDF2).</span>
          </div>
        </div>
      </div>

      <div class="acciones">
        <button onclick="guardarConfig()">💾 Guardar configuración</button>
        <button class="sec" onclick="probar('firebird')">Probar Firebird</button>
        <button class="sec" onclick="probar('servidor')">Probar servidor</button>
      </div>
      <div class="resultado" id="resultado-config"></div>
    </div>
  </div>

  <footer>Panel local del agente · solo accesible desde esta máquina (localhost) · PetrolRíos S.A.</footer>
</div>

<script>
let modoAuto = true;
let tabActual = 'monitoreo';

function fmtFecha(iso){ if(!iso) return '—'; return new Date(iso).toLocaleString('es-EC',{dateStyle:'short',timeStyle:'medium'}); }
function fmtUptime(s){ if(s<60) return Math.round(s)+' s'; if(s<3600) return Math.floor(s/60)+' min'; return Math.floor(s/3600)+' h '+Math.floor(s%3600/60)+' min'; }

function cambiarTab(tab){
  tabActual = tab;
  document.getElementById('tab-monitoreo').classList.toggle('active', tab==='monitoreo');
  document.getElementById('tab-config').classList.toggle('active', tab==='config');
  document.getElementById('vista-monitoreo').classList.toggle('hidden', tab!=='monitoreo');
  document.getElementById('vista-config').classList.toggle('hidden', tab!=='config');
  if(tab==='config') cargarConfig();
}

async function refrescar(){
  try{
    const r = await fetch('/api/estado');
    if(r.status === 401){ document.getElementById('overlay-login').classList.add('show'); sesionActiva=false; return; }
    const e = await r.json();
    modoAuto = e.modoAutomatico;
    document.getElementById('estacion').textContent = '· ' + (e.nombreEstacion || e.estacion);
    document.getElementById('dot-modo').className = 'dot' + (modoAuto ? '' : ' warn');
    document.getElementById('txt-modo').textContent = e.configurado ? (modoAuto ? 'Automático' : 'Manual') : 'Sin configurar';
    document.getElementById('toggle-modo').className = 'toggle' + (modoAuto ? ' on' : '');
    document.getElementById('banner-setup').classList.toggle('show', !e.configurado);

    document.getElementById('ultimo-ciclo').textContent = e.ultimoCiclo ? fmtFecha(e.ultimoCiclo) : '—';
    const res = document.getElementById('ultimo-resultado');
    res.textContent = e.ultimoResultado; res.style.color = e.ultimoCicloExitoso ? '' : 'var(--err)';
    document.getElementById('total-enviadas').textContent = e.totalTransaccionesEnviadas;
    document.getElementById('ciclos').textContent = e.ciclosEjecutados;
    document.getElementById('pendientes').textContent = e.lotesPendientes;
    document.getElementById('pendientes').style.color = e.lotesPendientes > 0 ? 'var(--warn)' : '';
    document.getElementById('latencia').textContent = e.ultimaLatenciaServidorMs != null ? e.ultimaLatenciaServidorMs + ' ms' : '—';
    document.getElementById('ultima-conexion').textContent =
      e.ultimaConexionServidor ? 'última conexión: ' + fmtFecha(e.ultimaConexionServidor)
      : (e.ultimaDesconexionServidor ? 'última desconexión: ' + fmtFecha(e.ultimaDesconexionServidor) : 'sin datos');

    document.getElementById('cfg-estacion').textContent = e.estacion;
    document.getElementById('cfg-nombre').textContent = e.nombreEstacion || '(sin nombre)';
    document.getElementById('cfg-servidor').textContent = e.servidor;
    document.getElementById('cfg-firebird').textContent = e.firebird;
    document.getElementById('cfg-intervalo').textContent = 'cada ' + e.intervaloSegundos + ' s';
    document.getElementById('cfg-watermark').textContent = fmtFecha(e.watermark);
    document.getElementById('cfg-uptime').textContent = fmtFecha(e.inicioAgente) + ' (hace ' + fmtUptime(e.uptimeSegundos) + ')';
    document.getElementById('cfg-version').textContent = e.versionAgente || '—';

    // Banner de actualización disponible
    const bu = document.getElementById('banner-update');
    if(e.actualizacionDisponible){
      document.getElementById('upd-version').textContent = e.versionDisponible || '';
      document.getElementById('upd-notas').textContent = e.notasActualizacion ? ('— ' + e.notasActualizacion) : '';
      const btn = document.getElementById('btn-aplicar-upd');
      btn.disabled = !!e.aplicandoActualizacion;
      btn.textContent = e.aplicandoActualizacion ? 'Aplicando…' : 'Aplicar y reiniciar';
      bu.classList.add('show');
    } else {
      bu.classList.remove('show');
    }

    document.getElementById('log').innerHTML = (e.eventos || []).map(ev =>
      `<div class="ev-${ev.nivel === 'OK' ? 'ok' : ev.nivel === 'ERROR' ? 'err' : 'info'}">` +
      `[${new Date(ev.fecha).toLocaleTimeString('es-EC')}] ${ev.nivel.padEnd(5)} ${ev.mensaje}</div>`
    ).join('') || 'sin eventos';
  }catch(err){
    document.getElementById('txt-modo').textContent = 'Agente sin respuesta';
    document.getElementById('dot-modo').className = 'dot err';
  }
}

async function cargarConfig(){
  try{
    const r = await fetch('/api/config'); const c = await r.json();
    document.getElementById('f-codigo').value = c.codigoEstacion || '';
    document.getElementById('f-nombre').value = c.nombreEstacion || '';
    document.getElementById('f-zona').value = c.zonaEstacion || '';
    document.getElementById('f-serverurl').value = c.serverUrl || '';
    document.getElementById('f-email').value = c.email || '';
    document.getElementById('f-timeout').value = c.serverTimeoutSegundos || 30;
    document.getElementById('f-fbhost').value = c.firebirdHost || '';
    document.getElementById('f-fbport').value = c.firebirdPort || 3050;
    document.getElementById('f-fbdatabase').value = c.firebirdDatabase || '';
    document.getElementById('f-fbuser').value = c.firebirdUser || '';
    document.getElementById('f-fbcharset').value = c.firebirdCharset || 'NONE';
    document.getElementById('f-fbdialect').value = c.firebirdDialect || 3;
    document.getElementById('f-fbwirecrypt').value = c.firebirdWireCrypt || 'Disabled';
    document.getElementById('f-intervalo').value = c.intervaloSegundos || 30;
    document.getElementById('f-auto').value = String(c.inicioAutomatico);
    document.getElementById('f-updateurl').value = c.updateFeedUrl || '';
    document.getElementById('f-updateurl2').value = c.updateFeedFallbackUrl || '';
    document.getElementById('f-localuser').value = c.panelLocalUsuario || '';
  }catch(e){}
}

function mostrarResultado(id, ok, texto){
  const el = document.getElementById(id);
  el.className = 'resultado ' + (ok ? 'ok' : 'err');
  el.textContent = (ok ? '✔ ' : '✘ ') + texto;
}

async function guardarConfig(){
  const payload = {
    codigoEstacion: document.getElementById('f-codigo').value,
    nombreEstacion: document.getElementById('f-nombre').value,
    zonaEstacion: document.getElementById('f-zona').value,
    serverUrl: document.getElementById('f-serverurl').value,
    email: document.getElementById('f-email').value,
    password: document.getElementById('f-password').value,
    serverTimeoutSegundos: parseInt(document.getElementById('f-timeout').value) || 30,
    firebirdHost: document.getElementById('f-fbhost').value,
    firebirdPort: parseInt(document.getElementById('f-fbport').value) || 3050,
    firebirdDatabase: document.getElementById('f-fbdatabase').value,
    firebirdUser: document.getElementById('f-fbuser').value,
    firebirdPassword: document.getElementById('f-fbpassword').value,
    firebirdCharset: document.getElementById('f-fbcharset').value,
    firebirdDialect: parseInt(document.getElementById('f-fbdialect').value) || 3,
    firebirdWireCrypt: document.getElementById('f-fbwirecrypt').value,
    intervaloSegundos: parseInt(document.getElementById('f-intervalo').value) || 30,
    inicioAutomatico: document.getElementById('f-auto').value === 'true',
    updateFeedUrl: document.getElementById('f-updateurl').value,
    updateFeedFallbackUrl: document.getElementById('f-updateurl2').value,
    panelLocalUsuario: document.getElementById('f-localuser').value,
    panelLocalPassword: document.getElementById('f-localpass').value
  };
  try{
    const r = await fetch('/api/config', {method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(payload)});
    if(r.ok){
      mostrarResultado('resultado-config', true, 'Configuración guardada. El agente ya está operando con estos datos.');
      document.getElementById('f-password').value = '';
      document.getElementById('f-fbpassword').value = '';
      document.getElementById('f-localpass').value = '';
      verificarSesion();
    } else {
      const j = await r.json();
      mostrarResultado('resultado-config', false, j.mensaje || 'No se pudo guardar.');
    }
  }catch(e){ mostrarResultado('resultado-config', false, 'No se pudo contactar al agente.'); }
  refrescar();
}

async function sincronizar(){
  const btn = document.getElementById('btn-sync');
  btn.disabled = true; btn.textContent = '⟳ Sincronizando…';
  try{ const r = await fetch('/api/sincronizar', {method:'POST'}); const j = await r.json(); mostrarResultado('resultado', j.ok, j.resultado); }
  catch(e){ mostrarResultado('resultado', false, 'No se pudo contactar al agente'); }
  btn.disabled = false; btn.textContent = '⟳ Sincronizar ahora';
  refrescar();
}

async function cambiarModo(){
  try{ await fetch('/api/modo', {method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({automatico: !modoAuto})}); }catch(e){}
  refrescar();
}

async function probar(cual){
  const id = tabActual === 'config' ? 'resultado-config' : 'resultado';
  mostrarResultado(id, true, 'Probando conexión…');
  try{ const r = await fetch('/api/probar-' + cual, {method:'POST'}); const j = await r.json(); mostrarResultado(id, j.ok, j.mensaje); }
  catch(e){ mostrarResultado(id, false, 'No se pudo contactar al agente'); }
  refrescar();
}

async function buscarActualizacion(){
  const btn = document.getElementById('btn-buscar-upd');
  btn.disabled = true; btn.textContent = 'Buscando…';
  try{
    const r = await fetch('/api/revisar-actualizacion', {method:'POST'});
    const j = await r.json();
    mostrarResultado('resultado', j.ok, j.mensaje || 'No se pudo consultar.');
  }catch(e){ mostrarResultado('resultado', false, 'No se pudo contactar al agente'); }
  btn.disabled = false; btn.textContent = 'Buscar actualización';
  refrescar();
}

async function aplicarActualizacion(){
  if(!confirm('Se descargará e instalará la nueva versión y el agente se reiniciará. ¿Continuar?')) return;
  const btn = document.getElementById('btn-aplicar-upd');
  btn.disabled = true; btn.textContent = 'Aplicando…';
  try{
    const r = await fetch('/api/actualizar', {method:'POST'});
    const j = await r.json();
    mostrarResultado('resultado', j.ok, j.mensaje);
  }catch(e){ mostrarResultado('resultado', true, 'El agente se está reiniciando para actualizar. Espere unos segundos y recargue.'); }
  refrescar();
}

// ─── Sesión / login del panel ───
let sesionActiva = false;

async function verificarSesion(){
  try{
    const r = await fetch('/api/sesion');
    const s = await r.json();
    sesionActiva = s.autenticado || s.requiereBootstrap;
    const overlay = document.getElementById('overlay-login');
    if(sesionActiva){
      overlay.classList.remove('show');
      const info = document.getElementById('sesion-info');
      if(s.autenticado){
        info.innerHTML = '👤 ' + (s.usuario||'') + ' · ' + (s.rol||'') +
          ' · <button class="link" onclick="hacerLogout()">salir</button>';
      } else {
        info.innerHTML = '<span style="color:var(--warn)">configuración inicial</span>';
      }
      return true;
    } else {
      overlay.classList.add('show');
      return false;
    }
  }catch(e){ document.getElementById('overlay-login').classList.add('show'); return false; }
}

async function hacerLogin(){
  const usuario = document.getElementById('login-usuario').value;
  const password = document.getElementById('login-password').value;
  mostrarResultado('resultado-login', true, 'Verificando…');
  try{
    const r = await fetch('/api/login', {method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({usuario, password})});
    const j = await r.json();
    if(j.ok){
      document.getElementById('login-password').value='';
      document.getElementById('resultado-login').className='resultado';
      await verificarSesion();
      refrescar();
    } else {
      mostrarResultado('resultado-login', false, j.mensaje || 'No se pudo iniciar sesión.');
    }
  }catch(e){ mostrarResultado('resultado-login', false, 'No se pudo contactar al agente.'); }
}

async function hacerLogout(){
  try{ await fetch('/api/logout', {method:'POST'}); }catch(e){}
  await verificarSesion();
}

async function iniciar(){
  const ok = await verificarSesion();
  if(ok) refrescar();
}
iniciar();
setInterval(() => { if(sesionActiva && tabActual==='monitoreo') refrescar(); }, 5000);
setInterval(verificarSesion, 60000);
</script>
</body>
</html>
""";
}
