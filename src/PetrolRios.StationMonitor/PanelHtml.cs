namespace PetrolRios.StationMonitor;

internal static class PanelHtml
{
    public const string Pagina = """
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>PetrolRíos — Monitor de estación</title>
  <style>
    :root{
      color-scheme:dark;--bg:#0a0a0a;--surface:#141414;--surface2:#1c1c1c;
      --line:#2a2a2a;--text:#fafafa;--muted:#a3a3a3;--green:#22c55e;
      --amber:#eab308;--red:#ef4444;--blue:#3b82f6;--primary:#3b82f6;
      --primary-strong:#2563eb;--shadow:0 16px 50px #00000080
    }
    *{box-sizing:border-box} body{margin:0;background:
      radial-gradient(1100px 480px at 18% -8%,#16233f 0,transparent 60%),
      var(--bg);
      color:var(--text);font:14px/1.5 Inter,Segoe UI,system-ui,sans-serif;min-height:100vh}
    button,input,select{font:inherit}.shell{max-width:1180px;margin:auto;padding:26px}
    header{display:flex;align-items:center;justify-content:space-between;gap:18px;margin-bottom:24px}
    .brand{display:flex;align-items:center;gap:14px}.logo{display:grid;place-items:center;width:48px;height:48px;
      border-radius:14px;background:linear-gradient(145deg,#3b82f6,#1e3a8a);font-size:24px;box-shadow:0 10px 30px #2563eb44}
    h1{font-size:22px;margin:0;font-weight:700}.eyebrow{font-size:11px;letter-spacing:.16em;text-transform:uppercase;color:var(--blue)}
    .status{display:flex;align-items:center;gap:9px;padding:9px 14px;border:1px solid var(--line);
      background:#141414cc;border-radius:999px}.dot{width:9px;height:9px;border-radius:50%;background:var(--amber)}
    .dot.ok{background:var(--green);box-shadow:0 0 12px var(--green)}.dot.err{background:var(--red)}
    .toolbar{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:18px;flex-wrap:wrap}
    .tabs{display:flex;gap:6px;padding:5px;background:#141414;border:1px solid var(--line);border-radius:12px}
    .tab{border:0;background:transparent;color:var(--muted);padding:9px 15px;border-radius:8px;cursor:pointer;font-weight:500}
    .tab.active{background:var(--surface2);color:var(--text)}
    .actions{display:flex;gap:8px;flex-wrap:wrap}button{border:0;border-radius:9px;padding:10px 15px;cursor:pointer;
      color:#fff;background:var(--primary);font-weight:600}button:hover{background:var(--primary-strong)}button.secondary{background:var(--surface2);color:var(--text);border:1px solid var(--line)}
    button.secondary:hover{background:#242424}button.warn{background:var(--amber);color:#1a1500}button:disabled{opacity:.55;cursor:wait}
    .hero{border:1px solid var(--line);background:linear-gradient(125deg,#15213acc,#141414dd);
      border-radius:18px;padding:22px;box-shadow:var(--shadow);margin-bottom:16px}
    .hero-top{display:flex;justify-content:space-between;gap:16px;align-items:start}.hero h2{font-size:24px;margin:3px 0}
    .hero p{color:var(--muted);margin:0}.clock{font-size:12px;color:var(--muted);text-align:right}
    .grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:16px 0}
    .metric,.card{border:1px solid var(--line);background:#141414;border-radius:14px;padding:16px}
    .metric .label,.card h3{font-size:11px;text-transform:uppercase;letter-spacing:.1em;color:var(--muted);margin:0 0 8px}
    .metric strong{font-size:28px}.metric strong.red{color:var(--red)}.metric strong.amber{color:var(--amber)}
    .metric strong.green{color:var(--green)}
    #problemas{display:grid;gap:10px}.problem{display:grid;grid-template-columns:auto 1fr auto;gap:13px;align-items:start;
      border:1px solid var(--line);background:#161616;border-radius:13px;padding:15px}
    .risk{width:10px;height:100%;min-height:64px;border-radius:99px;background:var(--amber)}
    .risk.critico{background:var(--red)}.risk.alto{background:#f97316}.risk.medio{background:var(--amber)}.risk.bajo{background:var(--blue)}
    .problem h3{font-size:15px;margin:0 0 5px}.problem p{margin:0;color:#cbd5e1}
    .meta{display:flex;gap:10px;flex-wrap:wrap;margin-top:9px;color:var(--muted);font-size:12px}
    .badge{padding:4px 8px;border-radius:999px;background:#1f2937;color:#cbd5e1;font-size:11px;font-weight:700;white-space:nowrap}
    .badge.critical{background:#3f1d1d;color:#fecaca}.empty{text-align:center;padding:55px 20px;border:1px dashed var(--line);
      border-radius:16px;color:var(--muted)}.empty .icon{font-size:42px;margin-bottom:8px;color:var(--green)}
    .hidden{display:none!important}.two{display:grid;grid-template-columns:1fr 1fr;gap:14px}
    label{display:block;color:var(--muted);font-size:12px;margin:10px 0 5px}
    input{width:100%;background:#0f0f0f;border:1px solid var(--line);color:var(--text);border-radius:9px;padding:10px 12px}
    input:focus{outline:2px solid #3b82f644;border-color:var(--primary)}.hint{font-size:11px;color:var(--muted);margin-top:5px}
    .result{display:none;margin-top:12px;padding:11px 13px;border-radius:9px}.result.show{display:block}
    .result.ok{background:#0f2e1f;color:#a7f3d0}.result.err{background:#3a1717;color:#fecaca}
    .event{display:grid;grid-template-columns:88px 70px 1fr;gap:10px;padding:9px 0;border-bottom:1px solid var(--line);font-size:12px}
    .event:last-child{border:0}.event time{color:var(--muted)}.event .ERROR{color:var(--red)}.event .ALERTA{color:var(--amber)}.event .OK{color:var(--green)}
    footer{text-align:center;color:var(--muted);font-size:11px;margin-top:20px}
    @media(max-width:850px){.grid{grid-template-columns:repeat(2,1fr)}.two{grid-template-columns:1fr}.problem{grid-template-columns:auto 1fr}.problem>.badge{grid-column:2}}
    @media(max-width:520px){.shell{padding:15px}header{align-items:flex-start}.status{font-size:12px}.grid{grid-template-columns:1fr 1fr}.hero-top{display:block}.clock{text-align:left;margin-top:10px}}
  </style>
</head>
<body>
<main class="shell">
  <header>
    <div class="brand">
      <div class="logo">⛽</div>
      <div><div class="eyebrow">Subsistema local</div><h1>Monitor de estación</h1></div>
    </div>
    <div class="status"><span class="dot" id="status-dot"></span><span id="status-text">Iniciando…</span></div>
  </header>

  <div class="toolbar">
    <nav class="tabs">
      <button class="tab active" id="tab-problemas" onclick="cambiarVista('problemas')">Problemas actuales</button>
      <button class="tab" id="tab-actividad" onclick="cambiarVista('actividad')">Actividad</button>
      <button class="tab" id="tab-config" onclick="cambiarVista('config')">Configuración</button>
    </nav>
    <div class="actions">
      <button class="secondary" id="btn-notif" onclick="activarNotificaciones()">🔔 Activar avisos</button>
      <button id="btn-refresh" onclick="actualizarAhora()">↻ Consultar ahora</button>
    </div>
  </div>

  <section id="vista-problemas">
    <div class="hero">
      <div class="hero-top">
        <div><div class="eyebrow" id="station-code">—</div><h2 id="station-name">Estación sin configurar</h2>
          <p>Solo se muestran problemas operativos activos reportados por el servidor central.</p></div>
        <div class="clock"><div>Última consulta</div><strong id="last-check">—</strong></div>
      </div>
      <div class="grid">
        <div class="metric"><div class="label">Problemas activos</div><strong class="amber" id="count-total">0</strong></div>
        <div class="metric"><div class="label">Críticos / altos</div><strong class="red" id="count-urgent">0</strong></div>
        <div class="metric"><div class="label">Nuevos</div><strong id="count-new">0</strong></div>
        <div class="metric"><div class="label">Conexión central</div><strong class="green" id="connection-short">—</strong></div>
      </div>
    </div>
    <div id="problemas"></div>
  </section>

  <section id="vista-actividad" class="hidden">
    <div class="card"><h3>Registro local de consultas y avisos</h3><div id="eventos"></div></div>
  </section>

  <section id="vista-config" class="hidden">
    <div class="card">
      <h3>Conexión de esta estación con el servidor central</h3>
      <p style="color:var(--muted)">Use una cuenta técnica asignada a esta estación. El monitor no puede escribir ni ver alertas de auditoría.</p>
      <div class="two">
        <div><label>Código de estación</label><input id="f-codigo" placeholder="EST-001"></div>
        <div><label>Servidor central</label><input id="f-server" placeholder="http://servidor:5170"></div>
        <div><label>Email de la cuenta de estación</label><input id="f-email" type="email"></div>
        <div><label>Contraseña</label><input id="f-password" type="password" placeholder="Dejar vacía para conservar"><div class="hint" id="password-hint"></div></div>
        <div><label>Consultar cada (segundos)</label><input id="f-interval" type="number" min="5" max="3600"></div>
        <div><label>Ventana de problemas (días)</label><input id="f-days" type="number" min="1" max="365"></div>
      </div>
      <div class="actions" style="margin-top:16px">
        <button onclick="guardarConfig()">Guardar y conectar</button>
        <button class="secondary" onclick="probarConexion()">Probar conexión</button>
      </div>
      <div class="result" id="config-result"></div>
    </div>
  </section>
  <footer>PetrolRíos · Monitor local de problemas operativos · Solo lectura</footer>
</main>
<script>
  let primeraCarga=true;
  let conocidos=new Set(JSON.parse(localStorage.getItem('problemasConocidos')||'[]'));
  let vista='problemas';

  const $=id=>document.getElementById(id);
  const esc=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const fecha=value=>value?new Date(value).toLocaleString('es-EC',{dateStyle:'short',timeStyle:'medium'}):'—';

  function cambiarVista(nueva){
    vista=nueva;
    for(const name of ['problemas','actividad','config']){
      $('vista-'+name).classList.toggle('hidden',name!==nueva);
      $('tab-'+name).classList.toggle('active',name===nueva);
    }
    if(nueva==='config')cargarConfig();
  }

  async function obtener(url,options){
    const response=await fetch(url,options);
    let data={};
    try{data=await response.json()}catch{}
    if(!response.ok)throw new Error(data.mensaje||'La operación no pudo completarse.');
    return data;
  }

  async function cargarEstado(){
    try{
      const state=await obtener('/api/estado');
      renderEstado(state);
      if(!state.configurado&&vista!=='config')cambiarVista('config');
    }catch(e){
      $('status-dot').className='dot err';$('status-text').textContent='Monitor local sin respuesta';
    }
  }

  function renderEstado(state){
    $('status-dot').className='dot '+(state.conectado?'ok':'err');
    $('status-text').textContent=state.conectado?'Central conectado':(state.ultimoError||'Sin conexión');
    $('station-code').textContent=state.codigoEstacion||'—';
    $('station-name').textContent=state.estacionNombre||state.codigoEstacion||'Estación sin configurar';
    $('last-check').textContent=fecha(state.ultimaConsulta);
    $('connection-short').textContent=state.conectado?'En línea':'Fuera';
    const problemas=state.problemas||[];
    $('count-total').textContent=problemas.length;
    $('count-urgent').textContent=problemas.filter(p=>['Critico','Alto'].includes(p.nivelRiesgo)).length;
    const nuevos=problemas.filter(p=>!conocidos.has(p.id));
    $('count-new').textContent=primeraCarga?0:nuevos.length;

    if(!primeraCarga&&nuevos.length){
      notificarNuevos(nuevos);
    }
    problemas.forEach(p=>conocidos.add(p.id));
    localStorage.setItem('problemasConocidos',JSON.stringify([...conocidos].slice(-1000)));
    primeraCarga=false;

    $('problemas').innerHTML=problemas.length?problemas.map(p=>`
      <article class="problem">
        <div class="risk ${esc(p.nivelRiesgo.toLowerCase())}"></div>
        <div><h3>${esc(p.descripcion)}</h3>
          <p>${esc(etiquetaTipo(p.tipoDetector))}</p>
          <div class="meta"><span>🕒 ${fecha(p.fechaDeteccion)}</span>
            ${p.empleadoCodigo?`<span>👤 ${esc(p.empleadoCodigo)}</span>`:''}
            ${p.transaccionReferencia?`<span>🔖 ${esc(p.transaccionReferencia)}</span>`:''}
            <span>Score ${Math.round(p.score)}/100</span></div>
        </div>
        <span class="badge ${p.nivelRiesgo==='Critico'?'critical':''}">${esc(p.nivelRiesgo)}</span>
      </article>`).join(''):`<div class="empty"><div class="icon">✓</div><strong>Sin problemas operativos activos</strong>
        <div>La estación está al día según la última consulta al central.</div></div>`;

    $('eventos').innerHTML=(state.eventos||[]).map(e=>`<div class="event">
      <time>${new Date(e.fecha).toLocaleTimeString('es-EC')}</time>
      <strong class="${esc(e.nivel)}">${esc(e.nivel)}</strong><span>${esc(e.mensaje)}</span></div>`).join('')
      ||'<div class="empty">Todavía no hay actividad registrada.</div>';
  }

  function etiquetaTipo(tipo){
    return ({CashFraud:'Control de turno y efectivo',InvoiceAnomaly:'Facturación y despachos',
      PaymentFraud:'Pagos',ComplianceViolation:'Cumplimiento',Custom:'Regla personalizada'})[tipo]||tipo;
  }

  function notificarNuevos(items){
    if(Notification.permission!=='granted')return;
    const first=items[0];
    new Notification(`${items.length} problema(s) nuevo(s)`,{
      body:items.length===1?first.descripcion:`${first.descripcion} y ${items.length-1} más`,
      tag:'petrolrios-problemas',renotify:true
    });
    try{
      const ctx=new AudioContext(),osc=ctx.createOscillator(),gain=ctx.createGain();
      osc.frequency.value=740;gain.gain.value=.08;osc.connect(gain);gain.connect(ctx.destination);
      osc.start();osc.stop(ctx.currentTime+.18);
    }catch{}
  }

  async function activarNotificaciones(){
    if(!('Notification'in window))return alert('Este navegador no admite notificaciones.');
    const permission=await Notification.requestPermission();
    $('btn-notif').textContent=permission==='granted'?'🔔 Avisos activos':'🔕 Avisos bloqueados';
    if(permission==='granted')new Notification('Avisos de PetrolRíos activados',{body:'Este equipo avisará cuando aparezcan problemas operativos nuevos.'});
  }

  async function actualizarAhora(){
    const btn=$('btn-refresh');btn.disabled=true;btn.textContent='Consultando…';
    try{await obtener('/api/actualizar',{method:'POST'});await cargarEstado()}
    catch(e){alert(e.message)}finally{btn.disabled=false;btn.textContent='↻ Consultar ahora'}
  }

  async function cargarConfig(){
    try{
      const c=await obtener('/api/config');
      $('f-codigo').value=c.codigoEstacion||'';$('f-server').value=c.serverUrl||'';
      $('f-email').value=c.email||'';$('f-interval').value=c.intervaloSegundos||15;
      $('f-days').value=c.diasConsulta||30;
      $('password-hint').textContent=c.tienePassword?'Ya existe una contraseña guardada.':'Falta configurar la contraseña.';
    }catch{}
  }

  function mostrarResultado(ok,mensaje){
    const el=$('config-result');el.className='result show '+(ok?'ok':'err');el.textContent=mensaje;
  }

  async function guardarConfig(){
    try{
      const result=await obtener('/api/config',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({
        codigoEstacion:$('f-codigo').value,serverUrl:$('f-server').value,email:$('f-email').value,
        password:$('f-password').value,intervaloSegundos:Number($('f-interval').value),
        diasConsulta:Number($('f-days').value)})});
      mostrarResultado(true,result.mensaje);$('f-password').value='';await cargarEstado();cambiarVista('problemas');
    }catch(e){mostrarResultado(false,e.message)}
  }

  async function probarConexion(){
    try{const r=await obtener('/api/probar',{method:'POST'});mostrarResultado(r.ok,r.mensaje)}
    catch(e){mostrarResultado(false,e.message)}
  }

  if('Notification'in window&&Notification.permission==='granted')$('btn-notif').textContent='🔔 Avisos activos';
  cargarEstado();setInterval(cargarEstado,5000);
</script>
</body>
</html>
""";
}
