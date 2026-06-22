using PetrolRios.Application.Interfaces;
using Serilog;

namespace PetrolRios.Api.Setup;

/// <summary>
/// Asistente de primer arranque. Si el sistema central arranca y NO tiene una base de datos
/// alcanzable, en vez de caerse levanta una pantalla web mínima (sin login, ya que aún no hay
/// base) para configurar y probar la conexión. Al guardar una conexión que funciona, persiste
/// config/connection.json y devuelve el control para que el sistema arranque normalmente.
///
/// Seguridad: solo se activa cuando no hay base alcanzable (no hay datos que proteger todavía) y
/// solo en Producción. Para entornos expuestos, conviene completar el primer arranque en una red
/// de confianza (LAN/VPN) o pre-configurar la conexión por variable de entorno.
/// </summary>
public static class SetupMode
{
    /// <summary>Datos del formulario de configuración (campos simples o cadena cruda).</summary>
    public sealed record ConexionForm(
        string? Cadena, string? Servidor, int? Puerto,
        string? BaseDatos, string? Usuario, string? Password, string? ModoSsl);

    public static async Task EjecutarAsync(string[] args, IConexionStore conexion)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Services.AddSingleton(conexion);

        var app = builder.Build();

        app.MapGet("/", () => Results.Content(PaginaHtml, "text/html"));

        app.MapPost("/setup/probar", async (ConexionForm form, IConexionStore c, CancellationToken ct) =>
        {
            var cs = ResolverCadena(form, c);
            if (string.IsNullOrWhiteSpace(cs))
                return Results.Ok(new { ok = false, mensaje = "Indique una cadena o los campos del servidor." });
            var (ok, mensaje, version) = await c.ProbarAsync(cs, ct);
            return Results.Ok(new { ok, mensaje, version });
        });

        app.MapPost("/setup/guardar", async (ConexionForm form, IConexionStore c, CancellationToken ct) =>
        {
            var cs = ResolverCadena(form, c);
            if (string.IsNullOrWhiteSpace(cs))
                return Results.BadRequest(new { ok = false, mensaje = "Indique una cadena o los campos del servidor." });
            var (ok, mensaje, _) = await c.ProbarAsync(cs, ct);
            if (!ok)
                return Results.BadRequest(new { ok = false, mensaje = $"No se guardó: la conexión falló ({mensaje})." });

            c.Guardar(cs);
            // Detener el modo configuración poco después de responder, para que el sistema arranque.
            _ = Task.Run(async () => { await Task.Delay(800); await app.StopAsync(); });
            return Results.Ok(new { ok = true, mensaje = "Conexión guardada. Iniciando el sistema..." });
        });

        // Cualquier otra ruta cae en la pantalla de configuración.
        app.MapFallback(() => Results.Redirect("/"));

        Log.Warning("Sin base de datos alcanzable. Modo de configuración inicial activo: abra el panel para configurar la conexión.");
        await app.RunAsync();
        Log.Information("Conexión configurada. Iniciando el sistema central...");
    }

    private static string? ResolverCadena(ConexionForm form, IConexionStore conexion)
    {
        if (!string.IsNullOrWhiteSpace(form.Cadena))
            return form.Cadena.Trim();

        if (!string.IsNullOrWhiteSpace(form.Servidor)
            && !string.IsNullOrWhiteSpace(form.BaseDatos)
            && !string.IsNullOrWhiteSpace(form.Usuario))
        {
            return conexion.ConstruirCadena(
                form.Servidor!.Trim(), form.Puerto ?? 5432, form.BaseDatos!.Trim(),
                form.Usuario!.Trim(), form.Password, form.ModoSsl ?? "Prefer");
        }

        return null;
    }

    private const string PaginaHtml = """
<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>Configuración inicial — PetrolRíos</title>
<style>
  :root { color-scheme: dark; }
  * { box-sizing: border-box; }
  body { margin:0; min-height:100vh; display:flex; align-items:center; justify-content:center;
    background:#0b1120; color:#e2e8f0; font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif; padding:24px; }
  .card { width:100%; max-width:560px; background:#0f172a; border:1px solid #1e293b; border-radius:16px; padding:28px; }
  h1 { font-size:20px; margin:0 0 4px; }
  p.sub { margin:0 0 20px; color:#94a3b8; font-size:14px; }
  label { display:block; font-size:12px; color:#94a3b8; margin:12px 0 4px; }
  input, select, textarea { width:100%; background:#0b1120; border:1px solid #334155; color:#e2e8f0;
    border-radius:8px; padding:10px 12px; font-size:14px; outline:none; }
  input:focus, select:focus, textarea:focus { border-color:#3b82f6; }
  .row { display:flex; gap:12px; } .row > div { flex:1; }
  .tabs { display:flex; gap:8px; margin-bottom:8px; }
  .tab { background:#0b1120; border:1px solid #334155; color:#94a3b8; border-radius:8px; padding:6px 12px; font-size:12px; cursor:pointer; }
  .tab.activo { border-color:#3b82f6; color:#93c5fd; background:rgba(59,130,246,.08); }
  .acciones { display:flex; gap:10px; margin-top:20px; }
  button { border:none; border-radius:8px; padding:10px 16px; font-size:14px; font-weight:600; cursor:pointer; }
  .sec { background:#0b1120; border:1px solid #334155; color:#e2e8f0; }
  .pri { background:#3b82f6; color:#fff; }
  button:disabled { opacity:.5; cursor:default; }
  .res { margin-top:16px; padding:12px; border-radius:8px; font-size:13px; display:none; }
  .ok { display:block; background:rgba(34,197,94,.1); border:1px solid rgba(34,197,94,.4); }
  .err { display:block; background:rgba(239,68,68,.1); border:1px solid rgba(239,68,68,.4); }
  .hide { display:none; }
</style>
</head>
<body>
  <div class="card">
    <h1>Configuración inicial</h1>
    <p class="sub">El sistema central no encuentra una base de datos. Indique dónde vive PostgreSQL para empezar.</p>

    <div class="tabs">
      <div class="tab activo" id="tab-campos" onclick="modo('campos')">Campos</div>
      <div class="tab" id="tab-cadena" onclick="modo('cadena')">Cadena (avanzado)</div>
    </div>

    <div id="campos">
      <div class="row">
        <div><label>Servidor (host o IP)</label><input id="servidor" placeholder="192.168.1.50 / db.empresa.com" /></div>
        <div><label>Puerto</label><input id="puerto" type="number" value="5432" /></div>
      </div>
      <div class="row">
        <div><label>Base de datos</label><input id="baseDatos" value="petrolrios" /></div>
        <div><label>Usuario</label><input id="usuario" value="petrolrios" /></div>
      </div>
      <div class="row">
        <div><label>Contraseña</label><input id="password" type="password" /></div>
        <div><label>SSL</label>
          <select id="ssl">
            <option value="Disable">Desactivado</option>
            <option value="Prefer" selected>Preferir</option>
            <option value="Require">Requerir</option>
            <option value="VerifyCA">Verificar CA</option>
            <option value="VerifyFull">Verificar completo</option>
          </select>
        </div>
      </div>
    </div>

    <div id="cadena" class="hide">
      <label>Cadena de conexión (Npgsql)</label>
      <textarea id="cad" rows="3" placeholder="Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require"></textarea>
    </div>

    <div class="acciones">
      <button class="sec" id="btn-probar" onclick="probar()">Probar conexión</button>
      <button class="pri" id="btn-guardar" onclick="guardar()">Guardar e iniciar</button>
    </div>

    <div class="res" id="res"></div>
  </div>

<script>
  let modoActual = 'campos';
  function modo(m){ modoActual=m;
    document.getElementById('campos').classList.toggle('hide', m!=='campos');
    document.getElementById('cadena').classList.toggle('hide', m!=='cadena');
    document.getElementById('tab-campos').classList.toggle('activo', m==='campos');
    document.getElementById('tab-cadena').classList.toggle('activo', m==='cadena');
  }
  function cuerpo(){
    if (modoActual==='cadena') return { cadena: document.getElementById('cad').value };
    return {
      servidor: document.getElementById('servidor').value,
      puerto: parseInt(document.getElementById('puerto').value)||5432,
      baseDatos: document.getElementById('baseDatos').value,
      usuario: document.getElementById('usuario').value,
      password: document.getElementById('password').value || null,
      modoSsl: document.getElementById('ssl').value
    };
  }
  function mostrar(ok, txt){ const r=document.getElementById('res'); r.className='res '+(ok?'ok':'err'); r.textContent=txt; }
  function ocupado(b){ document.getElementById('btn-probar').disabled=b; document.getElementById('btn-guardar').disabled=b; }
  async function probar(){ ocupado(true); mostrar(true,'Probando...');
    try{ const r=await fetch('/setup/probar',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(cuerpo())});
      const j=await r.json(); mostrar(j.ok, j.ok ? ('Conexión exitosa'+(j.version?' · PostgreSQL '+j.version:'')+'.') : (j.mensaje||'Falló la conexión.')); }
    catch(e){ mostrar(false,'No se pudo contactar al servidor.'); } finally{ ocupado(false); } }
  async function guardar(){ ocupado(true); mostrar(true,'Guardando...');
    try{ const r=await fetch('/setup/guardar',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(cuerpo())});
      const j=await r.json(); mostrar(j.ok, j.mensaje||'');
      if(j.ok){ setTimeout(()=>{ mostrar(true,'Sistema iniciando. Recargue esta página en unos segundos.'); }, 1200); } }
    catch(e){ mostrar(false,'No se pudo guardar.'); } finally{ ocupado(false); } }
</script>
</body>
</html>
""";
}
