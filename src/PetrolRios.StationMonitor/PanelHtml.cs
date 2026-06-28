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
      --amber:#eab308;--orange:#f97316;--red:#ef4444;--blue:#3b82f6;--violet:#8b5cf6;
      --primary:#3b82f6;--primary-strong:#2563eb;--shadow:0 16px 50px #00000080
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
      background:#141414cc;border-radius:999px}.dot{width:9px;height:9px;border-radius:50%;background:var(--amber);transition:.3s}
    .dot.ok{background:var(--green);box-shadow:0 0 12px var(--green)}.dot.err{background:var(--red);box-shadow:0 0 12px var(--red)}
    .toolbar{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:18px;flex-wrap:wrap}
    .tabs{display:flex;gap:6px;padding:5px;background:#141414;border:1px solid var(--line);border-radius:12px}
    .tab{border:0;background:transparent;color:var(--muted);padding:9px 15px;border-radius:8px;cursor:pointer;font-weight:500;position:relative}
    .tab.active{background:var(--surface2);color:var(--text)}
    .tab .pip{position:absolute;top:4px;right:6px;min-width:16px;height:16px;padding:0 4px;border-radius:9px;background:var(--red);
      color:#fff;font-size:10px;font-weight:800;display:none;place-items:center;line-height:16px}
    .tab .pip.show{display:grid}
    .actions{display:flex;gap:8px;flex-wrap:wrap}button{border:0;border-radius:9px;padding:10px 15px;cursor:pointer;
      color:#fff;background:var(--primary);font-weight:600;transition:.15s}button:hover{background:var(--primary-strong)}
    button.secondary{background:var(--surface2);color:var(--text);border:1px solid var(--line)}
    button.secondary:hover{background:#242424}button.warn{background:var(--amber);color:#1a1500}button:disabled{opacity:.55;cursor:wait}
    .hero{border:1px solid var(--line);background:linear-gradient(125deg,#15213acc,#141414dd);
      border-radius:18px;padding:22px;box-shadow:var(--shadow);margin-bottom:16px}
    .hero-top{display:flex;justify-content:space-between;gap:16px;align-items:start}.hero h2{font-size:24px;margin:3px 0}
    .hero p{color:var(--muted);margin:0}.clock{font-size:12px;color:var(--muted);text-align:right}
    .grid{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:16px 0 4px}
    .metric{border:1px solid var(--line);background:#141414;border-radius:14px;padding:16px;cursor:pointer;transition:.15s;position:relative}
    .metric:hover{border-color:#3a3a3a;background:#181818}.metric.sel{border-color:var(--primary);box-shadow:0 0 0 1px var(--primary) inset}
    .metric .label{font-size:11px;text-transform:uppercase;letter-spacing:.1em;color:var(--muted);margin:0 0 8px}
    .metric strong{font-size:28px}.metric strong.red{color:var(--red)}.metric strong.amber{color:var(--amber)}
    .metric strong.green{color:var(--green)}
    .card{border:1px solid var(--line);background:#141414;border-radius:14px;padding:16px}
    .card h3{font-size:11px;text-transform:uppercase;letter-spacing:.1em;color:var(--muted);margin:0 0 8px}
    /* distribución por severidad */
    .sevbar{display:flex;height:12px;border-radius:99px;overflow:hidden;background:#1f1f1f;margin:14px 0 10px}
    .sevbar span{display:block;height:100%;transition:width .4s}
    .sev-critico{background:var(--red)}.sev-alto{background:var(--orange)}.sev-medio{background:var(--amber)}.sev-bajo{background:var(--blue)}
    .legend{display:flex;gap:14px;flex-wrap:wrap;font-size:12px;color:var(--muted)}
    .legend i{display:inline-block;width:9px;height:9px;border-radius:3px;margin-right:5px;vertical-align:middle}
    .types{display:flex;gap:8px;flex-wrap:wrap;margin-top:12px}
    .typechip{display:flex;align-items:center;gap:6px;padding:6px 11px;border:1px solid var(--line);border-radius:999px;
      background:#161616;font-size:12px;color:#cbd5e1}.typechip b{color:var(--text)}
    /* filtros */
    .filters{display:flex;align-items:center;justify-content:space-between;gap:12px;margin:14px 0;flex-wrap:wrap}
    .pills{display:flex;gap:6px;flex-wrap:wrap}.pill{border:1px solid var(--line);background:#141414;color:var(--muted);
      padding:7px 13px;border-radius:999px;cursor:pointer;font-size:13px;font-weight:600;transition:.15s}
    .pill:hover{color:var(--text)}.pill.active{background:var(--surface2);color:var(--text);border-color:#3a3a3a}
    .pill .n{color:var(--muted);font-weight:700;margin-left:4px}
    .pill.active.crit{border-color:var(--red);color:#fecaca}.pill.active.alt{border-color:var(--orange);color:#fed7aa}
    .pill.active.med{border-color:var(--amber);color:#fde68a}.pill.active.baj{border-color:var(--blue);color:#bfdbfe}
    .search{display:flex;gap:8px;align-items:center}.search input{width:230px;background:#0f0f0f;border:1px solid var(--line);
      color:var(--text);border-radius:9px;padding:9px 12px}.search input:focus{outline:2px solid #3b82f644;border-color:var(--primary)}
    .search select{background:#0f0f0f;border:1px solid var(--line);color:var(--text);border-radius:9px;padding:9px 10px;cursor:pointer}
    #problemas{display:grid;gap:10px}
    .problem{border:1px solid var(--line);background:#161616;border-radius:13px;overflow:hidden;transition:.15s}
    .problem:hover{border-color:#3a3a3a}
    .problem.lv-critico{border-left:4px solid var(--red)}.problem.lv-alto{border-left:4px solid var(--orange)}
    .problem.lv-medio{border-left:4px solid var(--amber)}.problem.lv-bajo{border-left:4px solid var(--blue)}
    .phead{display:grid;grid-template-columns:1fr auto;gap:13px;align-items:start;padding:15px;cursor:pointer}
    .problem h3{font-size:15px;margin:0 0 6px;line-height:1.35}
    .ptype{display:inline-flex;align-items:center;gap:6px;font-size:12px;color:var(--muted)}
    .chips{display:flex;gap:7px;flex-wrap:wrap;margin-top:10px}
    .chip{display:inline-flex;align-items:center;gap:5px;padding:4px 9px;border-radius:8px;background:#1f2937;color:#cbd5e1;
      font-size:12px;max-width:280px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
    .chip b{color:#fff;font-weight:600}
    .meta{display:flex;gap:12px;flex-wrap:wrap;margin-top:10px;color:var(--muted);font-size:12px}
    .pright{display:flex;flex-direction:column;align-items:flex-end;gap:8px}
    .badge{padding:5px 10px;border-radius:999px;font-size:11px;font-weight:800;white-space:nowrap;letter-spacing:.03em}
    .badge.critico{background:#3f1d1d;color:#fecaca}.badge.alto{background:#3a230f;color:#fed7aa}
    .badge.medio{background:#332b0d;color:#fde68a}.badge.bajo{background:#10243f;color:#bfdbfe}
    .score{font-size:12px;color:var(--muted)}.score b{font-size:20px;color:var(--text)}
    .acc{display:inline-flex;align-items:center;gap:5px;padding:4px 9px;border-radius:999px;background:#3a1a3f;color:#f0abfc;
      font-size:11px;font-weight:800}
    .pdetail{display:none;padding:0 15px 15px 15px;border-top:1px solid var(--line);margin-top:2px}
    .problem.open .pdetail{display:block}
    .pdetail h4{font-size:11px;text-transform:uppercase;letter-spacing:.08em;color:var(--muted);margin:14px 0 8px}
    .kv{display:grid;grid-template-columns:auto 1fr;gap:6px 14px;font-size:13px}
    .kv dt{color:var(--muted)}.kv dd{margin:0;color:var(--text);word-break:break-word}
    .caret{transition:.2s;color:var(--muted);font-size:12px}.problem.open .caret{transform:rotate(180deg)}
    .empty{text-align:center;padding:55px 20px;border:1px dashed var(--line);border-radius:16px;color:var(--muted)}
    .empty .icon{font-size:42px;margin-bottom:8px;color:var(--green)}
    .hidden{display:none!important}.two{display:grid;grid-template-columns:1fr 1fr;gap:14px}
    label{display:block;color:var(--muted);font-size:12px;margin:10px 0 5px}
    input,select{font:inherit}.cfg input{width:100%;background:#0f0f0f;border:1px solid var(--line);color:var(--text);border-radius:9px;padding:10px 12px}
    .cfg input:focus{outline:2px solid #3b82f644;border-color:var(--primary)}.hint{font-size:11px;color:var(--muted);margin-top:5px}
    .result{display:none;margin-top:12px;padding:11px 13px;border-radius:9px}.result.show{display:block}
    .result.ok{background:#0f2e1f;color:#a7f3d0}.result.err{background:#3a1717;color:#fecaca}
    .event{display:grid;grid-template-columns:96px 70px 1fr;gap:10px;padding:9px 0;border-bottom:1px solid var(--line);font-size:12px}
    .event:last-child{border:0}.event time{color:var(--muted)}.event .ERROR{color:var(--red)}.event .ALERTA{color:var(--amber)}.event .OK{color:var(--green)}.event .INFO{color:var(--blue)}
    footer{text-align:center;color:var(--muted);font-size:11px;margin-top:20px}
    @media(max-width:850px){.grid{grid-template-columns:repeat(2,1fr)}.two{grid-template-columns:1fr}.search input{width:160px}}
    @media(max-width:520px){.shell{padding:15px}header{align-items:flex-start}.status{font-size:12px}.grid{grid-template-columns:1fr 1fr}
      .hero-top{display:block}.clock{text-align:left;margin-top:10px}.phead{grid-template-columns:1fr}.pright{flex-direction:row;align-items:center}.filters{align-items:stretch}}
  </style>
</head>
<body>
<main class="shell">
  <header>
    <div class="brand">
      <div class="logo">⛽</div>
      <div><div class="eyebrow">Subsistema local · solo lectura</div><h1>Monitor de estación</h1></div>
    </div>
    <div class="status"><span class="dot" id="status-dot"></span><span id="status-text">Iniciando…</span></div>
  </header>

  <div class="toolbar">
    <nav class="tabs">
      <button class="tab active" id="tab-problemas" onclick="cambiarVista('problemas')">Problemas actuales<span class="pip" id="pip-problemas"></span></button>
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
          <p>Problemas operativos activos que el servidor central detectó para esta estación.</p></div>
        <div class="clock"><div>Última consulta</div><strong id="last-check">—</strong>
          <div style="margin-top:6px" id="conn-line">—</div></div>
      </div>
      <div class="grid">
        <div class="metric" data-f="todos" onclick="filtrarNivel('todos')"><div class="label">Problemas activos</div><strong class="amber" id="count-total">0</strong></div>
        <div class="metric" data-f="urgentes" onclick="filtrarNivel('urgentes')"><div class="label">Críticos / altos</div><strong class="red" id="count-urgent">0</strong></div>
        <div class="metric"><div class="label">Nuevos (desde la última)</div><strong id="count-new">0</strong></div>
        <div class="metric"><div class="label">Acumulados (reincidencia)</div><strong class="amber" id="count-acc">0</strong></div>
      </div>
      <div class="sevbar" id="sevbar" title="Distribución por severidad"></div>
      <div class="legend" id="sevlegend"></div>
      <div class="types" id="types"></div>
    </div>

    <div class="filters">
      <div class="pills" id="nivel-pills"></div>
      <div class="search">
        <input id="buscar" placeholder="Buscar placa, RUC, cliente, factura…" oninput="render()">
        <select id="orden" onchange="render()">
          <option value="sev">Orden: severidad</option>
          <option value="reciente">Orden: más reciente</option>
          <option value="acc">Orden: más acumulados</option>
        </select>
      </div>
    </div>

    <div id="problemas"></div>
  </section>

  <section id="vista-actividad" class="hidden">
    <div class="card"><h3>Registro local de consultas y avisos</h3><div id="eventos"></div></div>
  </section>

  <section id="vista-config" class="hidden">
    <div class="card cfg">
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
  let problemas=[];           // último set recibido (enriquecido)
  let nuevosIds=new Set();     // ids nuevos desde la última consulta
  let filtroNivel=localStorage.getItem('mon.nivel')||'todos';
  let estadoConectado=false;

  const $=id=>document.getElementById(id);
  const esc=v=>String(v??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const fecha=v=>v?new Date(v).toLocaleString('es-EC',{dateStyle:'short',timeStyle:'medium'}):'—';
  const hora=v=>v?new Date(v).toLocaleTimeString('es-EC'):'—';
  const lower=v=>String(v||'').toLowerCase();
  const cap=v=>{v=String(v||'');return v.charAt(0).toUpperCase()+v.slice(1).toLowerCase();};

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
      estadoConectado=false;
      $('status-dot').className='dot err';$('status-text').textContent='Monitor local sin respuesta';
    }
  }

  const TIPOS={CashFraud:'Control de turno y efectivo',InvoiceAnomaly:'Facturación y despachos',
    PaymentFraud:'Pagos',ComplianceViolation:'Cumplimiento',Custom:'Regla personalizada'};
  const ICON={CashFraud:'💵',InvoiceAnomaly:'🧾',PaymentFraud:'💳',ComplianceViolation:'⚖️',Custom:'⚙️'};
  const NIVELES=[['critico','Crítico','crit'],['alto','Alto','alt'],['medio','Medio','med'],['bajo','Bajo','baj']];
  const RANK={critico:4,alto:3,medio:2,bajo:1};

  // Evidencia útil que pueda venir en el metadata de la alerta (claves variables según la regla).
  function parseMeta(json){try{return json?JSON.parse(json):{}}catch{return{}}}
  const valor=v=>Array.isArray(v)?v.filter(x=>String(x).trim()).join(', '):String(v??'');
  function primera(meta,claves){for(const k of claves){for(const real of Object.keys(meta)){
    if(real.toLowerCase()===k.toLowerCase()){const t=valor(meta[real]);if(t&&t!=='0')return t;}}}return '';}

  function chips(meta){
    const defs=[
      ['🚗','Placa',['Placa','Placas']],
      ['🧾','RUC',['RucCliente','Ruc','Rucs']],
      ['👥','Cliente',['Cliente','Clientes','CodigoCliente']],
      ['📄','Documento',['NumeroDocumento','NumerosFactura','Documento']],
      ['⛽','Productos',['Productos','Producto']],
      ['🛢️','Galones',['Galones']],
      ['💲','Monto',['MontoTotal','Monto']],
      ['⚖️','Diferencia',['Diferencia']],
      ['🔁','Veces',['CantidadFacturas','Conteo']],
    ];
    const out=[];
    for(const [ic,lbl,keys] of defs){const t=primera(meta,keys);if(t)out.push(
      `<span class="chip" title="${esc(lbl)}: ${esc(t)}">${ic} <b>${esc(lbl)}</b> ${esc(t)}</span>`);}
    return out.join('');
  }

  function renderEstado(state){
    estadoConectado=!!state.conectado;
    $('status-dot').className='dot '+(state.conectado?'ok':'err');
    $('status-text').textContent=state.conectado?'Central conectado':(state.ultimoError||'Sin conexión');
    $('station-code').textContent=state.codigoEstacion||'—';
    $('station-name').textContent=state.estacionNombre||state.codigoEstacion||'Estación sin configurar';
    $('last-check').textContent=fecha(state.ultimaConsulta);
    $('conn-line').innerHTML=state.conectado?'<span style="color:var(--green)">● En línea</span>':'<span style="color:var(--red)">● Fuera de línea</span>';

    const lista=(state.problemas||[]).map(p=>({...p,_meta:parseMeta(p.metadataJson),
      _acc:Number((parseMeta(p.metadataJson).EventosAcumulados)||0)}));
    nuevosIds=new Set(lista.filter(p=>!conocidos.has(p.id)).map(p=>p.id));

    // métricas
    $('count-total').textContent=lista.length;
    $('count-urgent').textContent=lista.filter(p=>['critico','alto'].includes(lower(p.nivelRiesgo))).length;
    $('count-new').textContent=primeraCarga?0:nuevosIds.size;
    $('count-acc').textContent=lista.filter(p=>(p.transaccionReferencia||'').startsWith('RAPIDOS')||p._acc>1).length;

    // distribución por severidad
    const por={critico:0,alto:0,medio:0,bajo:0};
    lista.forEach(p=>{const k=lower(p.nivelRiesgo);if(k in por)por[k]++;});
    const tot=lista.length||1;
    $('sevbar').innerHTML=NIVELES.map(([k])=>por[k]?`<span class="sev-${k}" style="width:${por[k]/tot*100}%" title="${cap(k)}: ${por[k]}"></span>`:'').join('');
    $('sevlegend').innerHTML=NIVELES.map(([k,lbl])=>`<span><i class="sev-${k}"></i>${lbl} ${por[k]}</span>`).join('');

    // chips por tipo
    const tipos={};lista.forEach(p=>{tipos[p.tipoDetector]=(tipos[p.tipoDetector]||0)+1;});
    $('types').innerHTML=Object.entries(tipos).sort((a,b)=>b[1]-a[1]).map(([t,n])=>
      `<span class="typechip">${ICON[t]||'•'} ${esc(TIPOS[t]||t)} <b>${n}</b></span>`).join('')||'';

    // pills de nivel
    $('nivel-pills').innerHTML=
      `<button class="pill ${filtroNivel==='todos'?'active':''}" onclick="filtrarNivel('todos')">Todos <span class="n">${lista.length}</span></button>`+
      NIVELES.map(([k,lbl,cls])=>`<button class="pill ${cls} ${filtroNivel===k?'active':''}" onclick="filtrarNivel('${k}')">${lbl} <span class="n">${por[k]}</span></button>`).join('');

    // avisos del SO
    if(!primeraCarga&&nuevosIds.size)notificarNuevos(lista.filter(p=>nuevosIds.has(p.id)));
    lista.forEach(p=>conocidos.add(p.id));
    localStorage.setItem('problemasConocidos',JSON.stringify([...conocidos].slice(-1000)));
    const pip=$('pip-problemas');const urg=lista.filter(p=>['critico','alto'].includes(lower(p.nivelRiesgo))).length;
    pip.textContent=urg;pip.classList.toggle('show',urg>0&&vista!=='problemas');
    primeraCarga=false;

    problemas=lista;
    render();

    $('eventos').innerHTML=(state.eventos||[]).map(e=>`<div class="event">
      <time>${hora(e.fecha)}</time><strong class="${esc(e.nivel)}">${esc(e.nivel)}</strong><span>${esc(e.mensaje)}</span></div>`).join('')
      ||'<div class="empty">Todavía no hay actividad registrada.</div>';
  }

  function filtrarNivel(n){
    filtroNivel=n;localStorage.setItem('mon.nivel',n);
    document.querySelectorAll('.metric').forEach(m=>m.classList.toggle('sel',m.dataset.f===n));
    render();
  }

  function render(){
    const q=lower($('buscar').value.trim());
    const orden=$('orden').value;
    let lista=problemas.slice();

    if(filtroNivel==='urgentes')lista=lista.filter(p=>['critico','alto'].includes(lower(p.nivelRiesgo)));
    else if(filtroNivel!=='todos')lista=lista.filter(p=>lower(p.nivelRiesgo)===filtroNivel);

    if(q)lista=lista.filter(p=>{
      const hay=[p.descripcion,p.empleadoCodigo,p.transaccionReferencia,JSON.stringify(p._meta)].join(' ').toLowerCase();
      return hay.includes(q);
    });

    if(orden==='reciente')lista.sort((a,b)=>new Date(b.fechaDeteccion)-new Date(a.fechaDeteccion));
    else if(orden==='acc')lista.sort((a,b)=>(b._acc||0)-(a._acc||0)||b.score-a.score);
    else lista.sort((a,b)=>(RANK[lower(b.nivelRiesgo)]||0)-(RANK[lower(a.nivelRiesgo)]||0)||b.score-a.score);

    // sincronizar pills/metrics seleccionadas
    document.querySelectorAll('#nivel-pills .pill').forEach(el=>{});
    document.querySelectorAll('.metric').forEach(m=>m.classList.toggle('sel',m.dataset.f===filtroNivel));

    if(!lista.length){
      $('problemas').innerHTML=problemas.length
        ?`<div class="empty"><div class="icon" style="color:var(--blue)">🔍</div><strong>Ningún problema coincide con el filtro</strong><div>Ajuste la búsqueda o el nivel.</div></div>`
        :`<div class="empty"><div class="icon">✓</div><strong>Sin problemas operativos activos</strong><div>La estación está al día según la última consulta al central.</div></div>`;
      return;
    }

    $('problemas').innerHTML=lista.map(p=>{
      const nv=lower(p.nivelRiesgo);
      const ev=chips(p._meta);
      const acc=(p.transaccionReferencia||'').startsWith('RAPIDOS')&&p._acc>1
        ?`<span class="acc">🔁 ×${p._acc} acumulados</span>`:'';
      const kvs=Object.entries(p._meta).filter(([,v])=>valor(v)!=='').map(([k,v])=>
        `<dt>${esc(k)}</dt><dd>${esc(valor(v))}</dd>`).join('');
      return `<article class="problem lv-${nv}" id="prob-${p.id}">
        <div class="phead" onclick="toggleDetalle(${p.id})">
          <div>
            <h3>${esc(p.descripcion)}</h3>
            <span class="ptype">${ICON[p.tipoDetector]||'•'} ${esc(TIPOS[p.tipoDetector]||p.tipoDetector)}</span>
            ${ev?`<div class="chips">${ev}</div>`:''}
            <div class="meta"><span>🕒 ${fecha(p.fechaDeteccion)}</span>
              ${p.empleadoCodigo?`<span>👤 ${esc(p.empleadoCodigo)}</span>`:''}
              ${nuevosIds.has(p.id)?'<span style="color:var(--blue)">● nuevo</span>':''}</div>
          </div>
          <div class="pright">
            <span class="badge ${nv}">${esc(cap(p.nivelRiesgo))}</span>
            ${acc}
            <span class="score"><b>${Math.round(p.score)}</b>/100</span>
            <span class="caret">▾</span>
          </div>
        </div>
        <div class="pdetail">
          <h4>Evidencia y detalle</h4>
          <dl class="kv">
            ${kvs||'<dt>Detalle</dt><dd>Sin metadatos adicionales.</dd>'}
            ${p.transaccionReferencia?`<dt>Referencia</dt><dd>${esc(p.transaccionReferencia)}</dd>`:''}
            <dt>Ámbito</dt><dd>${esc(p.ambito)}</dd>
            <dt>Estado</dt><dd>${esc(p.estado)}</dd>
          </dl>
        </div>
      </article>`;}).join('');
  }

  function toggleDetalle(id){const el=$('prob-'+id);if(el)el.classList.toggle('open');}

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
  filtrarNivel(filtroNivel);
  cargarEstado();setInterval(cargarEstado,5000);
</script>
</body>
</html>
""";
}
