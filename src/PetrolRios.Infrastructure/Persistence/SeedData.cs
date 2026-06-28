using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.Programacion;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PetrolRiosDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            await SeedRolesAsync(context);
            await SeedUsuarioAdminAsync(context, config);
            await SeedEstacionesAsync(context);
            await SeedReglasDeteccionAsync(context);
            await SeedAgentUsersAsync(context);

            await context.SaveChangesAsync();
        }

        // Pasos idempotentes: aplican también sobre bases ya sembradas
        await EnsureReglasNuevasAsync(context);
        await EnsureRelacionesTablaAsync(context);
        await EnsureUsuariosDemoAsync(context);
        await EnsureRolAgenteAsync(context);
        await EnsureAgentUsersStationAssignmentAsync(context);
        await EnsureCuentasAccesoAsync(context, config);
        await EnsurePreciosCombustibleAsync(context);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Siembra los precios oficiales vigentes de los combustibles regulados de Ecuador (Extra, Ecopaís,
    /// Diésel) si la tabla está vacía. Valores oficiales del 12-jun-2026 al 11-jul-2026 (EP Petroecuador,
    /// sistema de bandas). El administrador los actualiza cada mes desde la API/panel; no se pisan los
    /// existentes (idempotente). La Súper se excluye a propósito: su precio no es regulado.
    /// </summary>
    private static async Task EnsurePreciosCombustibleAsync(PetrolRiosDbContext context)
    {
        if (await context.PreciosCombustible.AnyAsync()) return;

        const string fuente = "EP Petroecuador — sistema de bandas (vigente jun 2026)";
        var desde = new DateTime(2026, 6, 12);
        var hasta = new DateTime(2026, 7, 11);

        await context.PreciosCombustible.AddRangeAsync(
            PrecioCombustible.Create(TipoCombustible.Extra, 3.31m, 1.021m, desde, hasta, fuente),
            PrecioCombustible.Create(TipoCombustible.Ecopais, 3.31m, 1.650m, desde, hasta, fuente),
            PrecioCombustible.Create(TipoCombustible.Diesel, 3.25m, 1.602m, desde, hasta, fuente));

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Siembra (idempotente) las relaciones clave entre tablas para enriquecer las alertas de las
    /// reglas personalizadas. La principal: un Despacho (DetalleFactura) se relaciona con su Factura
    /// por el código de cliente, de modo que una regla sobre despachos pueda mostrar la placa, el
    /// vendedor, el cliente y el número de factura (que viven en la factura). El Admin puede agregar
    /// más desde la interfaz.
    /// </summary>
    private static async Task EnsureRelacionesTablaAsync(PetrolRiosDbContext context)
    {
        var deseadas = new[]
        {
            RelacionTabla.Create("DetalleFactura", "Factura", "CodigoCliente", "CodigoCliente",
                "Factura del despacho (placa, vendedor, cliente, N° de factura)"),
            RelacionTabla.Create("Factura", "DetalleFactura", "CodigoCliente", "CodigoCliente",
                "Despacho de la factura (galones, producto)"),
        };

        foreach (var rel in deseadas)
        {
            var existe = await context.RelacionesTabla.AnyAsync(r =>
                r.FuenteOrigen == rel.FuenteOrigen && r.FuenteDestino == rel.FuenteDestino
                && r.CampoOrigen == rel.CampoOrigen && r.CampoDestino == rel.CampoDestino);
            if (!existe) await context.RelacionesTabla.AddAsync(rel);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Garantiza que las cuentas puedan iniciar sesión, evitando bloqueos accidentales:
    ///  - Si NO se exige verificación por correo (caso por defecto), marca como verificadas las
    ///    cuentas activas que quedaron sin verificar (p. ej. creadas antes de este cambio), para
    ///    que no queden bloqueadas ni aparezcan como "no verificadas".
    ///  - Asegura que el Administrador esté activo y sin bloqueo por intentos fallidos.
    ///  - Recuperación "break-glass": si <c>Seguridad:AdminPasswordInicial</c> tiene valor,
    ///    restablece la contraseña del Administrador a ese valor y obliga a cambiarla, por si se
    ///    perdió el acceso. Con el valor vacío (por defecto) no toca la contraseña existente.
    /// </summary>
    private static async Task EnsureCuentasAccesoAsync(PetrolRiosDbContext context, IConfiguration config)
    {
        var requiereVerificacion = config.GetValue("Seguridad:RequerirVerificacionEmail", false);
        if (!requiereVerificacion)
        {
            var sinVerificar = await context.Usuarios
                .Where(u => u.Activo && !u.EmailVerificado)
                .ToListAsync();
            foreach (var usuario in sinVerificar)
                usuario.MarcarEmailVerificado();
        }

        var admin = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@petrolrios.com");
        if (admin is null) return;

        admin.Activo = true;
        admin.ResetearFallos(); // un bloqueo por intentos no debe dejar afuera al Administrador

        var passwordRecuperacion = config["Seguridad:AdminPasswordInicial"];
        if (!string.IsNullOrWhiteSpace(passwordRecuperacion))
        {
            admin.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(passwordRecuperacion));
            admin.DebeCambiarPassword = true;
            admin.MarcarEmailVerificado();
        }
    }

    /// <summary>
    /// Mantiene las cuentas técnicas del agente vinculadas a su estación. Además de permitir
    /// que el Monitor de estación reutilice esas credenciales, evita que una cuenta técnica
    /// pueda consultar problemas operativos de otra estación.
    /// </summary>
    private static async Task EnsureAgentUsersStationAssignmentAsync(PetrolRiosDbContext context)
    {
        var estaciones = await context.Estaciones
            .AsNoTracking()
            .Select(e => new { e.Id, e.Codigo })
            .ToListAsync();

        var agentes = await context.Usuarios
            .Where(u => u.Email.StartsWith("agent-"))
            .ToListAsync();

        foreach (var estacion in estaciones)
        {
            var email = $"agent-{estacion.Codigo.ToLowerInvariant()}@petrolrios.com";
            var agente = agentes.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (agente is not null && agente.EstacionId != estacion.Id)
                agente.AsignarEstacion(estacion.Id);
        }
    }

    /// <summary>
    /// Asegura el rol "Agente" (cuenta de servicio del Station Agent, SIN acceso a la app central) y
    /// re-asigna a ese rol las cuentas de agente (agent-*) que vinieran con otro rol (antes Auditor).
    /// Idempotente: corre en cada arranque para corregir bases ya existentes — defensa en profundidad:
    /// un agente nunca debe poder entrar al central como auditor.
    /// </summary>
    private static async Task EnsureRolAgenteAsync(PetrolRiosDbContext context)
    {
        var agenteRol = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Agente");
        if (agenteRol is null)
        {
            agenteRol = Rol.Create(
                "Agente",
                "Agente de estacion - cuenta de servicio del Station Agent: solo conecta y envia datos, SIN acceso a la app central");
            await context.Roles.AddAsync(agenteRol);
            await context.SaveChangesAsync();
        }

        var agentes = await context.Usuarios
            .Where(u => u.Email.StartsWith("agent-") && u.RolId != agenteRol.Id)
            .ToListAsync();
        foreach (var a in agentes)
            a.ActualizarPerfil(null, agenteRol.Id);
        if (agentes.Count > 0)
            await context.SaveChangesAsync();
    }

    /// <summary>
    /// Agrega las reglas nuevas que no existan (post-revisión del jurado) y desactiva
    /// la regla de fuera de horario en bases existentes (las estaciones operan 24/7).
    /// </summary>
    private static async Task EnsureReglasNuevasAsync(PetrolRiosDbContext context)
    {
        var existentes = await context.ReglasDeteccion
            .Select(r => r.ParametroNombre)
            .ToListAsync();

        var nuevas = new List<ReglaDeteccion>();

        void AddIfMissing(TipoDetector tipo, string nombre, string descripcion, string parametro, double umbral,
            AmbitoAlerta ambito = AmbitoAlerta.Auditoria)
        {
            if (!existentes.Contains(parametro))
                nuevas.Add(ReglaDeteccion.Create(tipo, nombre, descripcion, parametro, umbral, ambito));
        }

        AddIfMissing(TipoDetector.CashFraud,
            "Venta a credito sin cliente identificado",
            "Genera alerta si una venta a credito no tiene cliente o identificacion registrada (posible venta en efectivo registrada como credito)",
            "CreditoSinClienteHabilitado", 1.0);
        AddIfMissing(TipoDetector.CashFraud,
            "Proporcion atipica de efectivo corporativo",
            "Genera alerta si un vendedor supera el porcentaje umbral de ventas en efectivo sobre clientes corporativos (patron del caso documentado de enero 2026)",
            "EfectivoCorporativoPorcentajeUmbral", 30.0);
        AddIfMissing(TipoDetector.CashFraud,
            "Turno sin cerrar",
            "Genera una alerta operativa (para el administrador de la estacion) si un turno sigue abierto desde hace mas horas que el umbral. Umbral = horas.",
            "TurnoSinCerrarHorasUmbral", 18.0, AmbitoAlerta.Operativa);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Descuento excesivo fuera de politica",
            "Genera alerta si el descuento aplicado excede el porcentaje maximo permitido por la politica comercial",
            "DescuentoPorcentajeMaximo", 10.0);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Total de factura inconsistente",
            "Genera alerta si el total registrado no corresponde a subtotal - descuento + IVA (indicador de manipulacion documental)",
            "TotalInconsistenteHabilitado", 1.0);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Fecha fuera de rango plausible (backdating)",
            "Genera alerta si una factura o credito esta fechado en el futuro mas alla de la tolerancia (en horas) respecto al procesamiento; senial de manipulacion de fecha. Umbral = horas de tolerancia.",
            "FechaFuturaToleranciaHoras", 24.0);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Despacho no facturado",
            "DESHABILITADA por defecto: FAC_DESP es la FORMA DE PAGO del despacho (contado/tarjeta/credito/cheque), no un indicador de 'facturado', por lo que la heuristica daba falsos positivos. La deteccion correcta exige cruzar DESP.NUM_DESP con DCTO.NDO_DCTO sobre el staging con periodo de gracia (trabajo futuro, analogo al cuadre de liquidacion).",
            "DespachoNoFacturadoHabilitado", 0.0, AmbitoAlerta.Operativa);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Anulaciones recurrentes (kiting)",
            "Genera alerta si un mismo punto de emision tiene anulaciones en varios dias distintos (umbral = dias minimos); posible patron de cancelar y reingresar para rodar la deuda o mover el periodo.",
            "AnulacionRecurrenteDiasMinimo", 3.0);
        AddIfMissing(TipoDetector.PaymentFraud,
            "Despachos rapidos sucesivos",
            "Genera alerta si el mismo RUC/cliente (o la misma placa) registra 2 o mas despachos consecutivos con menos de N minutos entre ellos. Es DINAMICA en la llave (RUC si no cambian el cliente; placa si reutilizan el vehiculo) y ACUMULABLE: en vez de una alerta por racha, se acumulan en UNA por caso que escala por cantidad (2-3 Medio, 4-5 Alto, 6+ Critico) y re-emerge arriba. Ambito Ambos (operativa + auditoria).",
            "DespachosRapidosMinutosUmbral", 10.0, AmbitoAlerta.Ambos);
        AddIfMissing(TipoDetector.PaymentFraud,
            "Credito sin garante",
            "Genera alerta si un credito (CRED_CABE) se otorga sin garante (COD_GARA vacio); senial de autorizacion indebida de credito.",
            "CreditoSinGaranteHabilitado", 1.0);
        AddIfMissing(TipoDetector.ComplianceViolation,
            "Venta sin placa en monto mayor",
            "Genera alerta si una venta supera el monto umbral sin placa registrada (trazabilidad exigida por normativa de comercializacion)",
            "VentaSinPlacaMontoMinimo", 200.0);
        AddIfMissing(TipoDetector.ComplianceViolation,
            "Venta sin identificacion del cliente",
            "Genera alerta si una venta supera el monto umbral sin cedula/RUC del cliente; el SRI (Resolucion NAC-DGERCGC13-00382) exige registrar la identificacion del comprador en facturas de combustible.",
            "VentaSinIdentificacionMontoMinimo", 50.0);
        AddIfMissing(TipoDetector.ComplianceViolation,
            "Despacho de alto volumen sin placa",
            "Genera alerta si un despacho sin placa registrada supera el umbral de galones; patron tipico de desvio de combustible que la ARCERNNR controla con cupos y trazabilidad por placa.",
            "GalonesSinPlacaMaximo", 20.0);

        // Placa reutilizada en el día: cuenta por jornada, así que NO corre "cada ciclo" (vería solo el
        // lote incremental de minutos) sino una vez al día sobre la ventana del día (Calendario Diario
        // 23:55, hora de estación). Por eso se siembra con programación, fuera de AddIfMissing.
        if (!existentes.Contains("PlacaReutilizadaDiaUmbral"))
        {
            var placaReutilizada = ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Placa reutilizada en el dia",
                "Genera alerta si una misma placa se factura mas de N veces en el mismo dia (reutilizacion de placa; caso real de 14 facturas en un dia). Corre una vez al dia sobre la ventana del dia. Umbral configurable: auditoria sugiere bajarlo hasta 2 segun la tolerancia a falsos positivos.",
                "PlacaReutilizadaDiaUmbral",
                5.0);
            placaReutilizada.ProgramacionJson = new ProgramacionEjecucion
            {
                Modo = ModoProgramacion.Calendario,
                CalendarioTipo = TipoCalendario.Diario,
                Hora = 23,
                Minuto = 55
            }.Serializar();
            nuevas.Add(placaReutilizada);
        }

        // Factura fuera de liquidación / cuadre de turno (mejora #3): al cerrar un turno, sus facturas
        // deben quedar liquidadas (LIQU). La liquidación llega DESPUÉS del cierre (otro lote), así que no
        // es un detector de ventana: lo evalúa CuadreLiquidacionService sobre el staging acumulado, una vez
        // al día (Calendario Diario 23:50). Umbral = horas de gracia tras el cierre antes de alertar.
        if (!existentes.Contains("FacturaSinLiquidacionHorasUmbral"))
        {
            var cuadre = ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Factura fuera de liquidacion (cuadre de turno)",
                "Genera alerta si un turno CERRADO no aparece en la liquidacion (LIQU) y tiene facturas: combustible facturado que quedo fuera del cuadre de caja. Corre una vez al dia sobre el staging acumulado (30 dias). Umbral = horas de gracia tras el cierre del turno antes de alertar (da tiempo a que llegue su liquidacion).",
                "FacturaSinLiquidacionHorasUmbral",
                12.0);
            cuadre.ProgramacionJson = new ProgramacionEjecucion
            {
                Modo = ModoProgramacion.Calendario,
                CalendarioTipo = TipoCalendario.Diario,
                Hora = 23,
                Minuto = 50
            }.Serializar();
            nuevas.Add(cuadre);
        }

        if (nuevas.Count > 0)
            await context.ReglasDeteccion.AddRangeAsync(nuevas);

        // Desactivar fuera de horario en bases existentes (estaciones 24/7)
        var fueraHorario = await context.ReglasDeteccion
            .FirstOrDefaultAsync(r => r.ParametroNombre == "FueraHorarioHabilitado");
        if (fueraHorario is not null && fueraHorario.Activa)
        {
            fueraHorario.Activa = false;
            fueraHorario.ValorUmbral = 0.0;
        }

        // Desactivar "Despacho no facturado": FAC_DESP es la FORMA DE PAGO del despacho, no un indicador de
        // "facturado", así que la heurística generaba falsos positivos (p. ej. ventas de contado con FAC_DESP='0').
        // La detección correcta exige cruzar DESP.NUM_DESP ↔ DCTO.NDO_DCTO sobre el staging acumulado y con
        // periodo de gracia (un servicio análogo a CuadreLiquidacionService), no por lote. Hasta tenerlo, off.
        var despachoNoFacturado = await context.ReglasDeteccion
            .FirstOrDefaultAsync(r => r.ParametroNombre == "DespachoNoFacturadoHabilitado");
        if (despachoNoFacturado is not null && despachoNoFacturado.Activa)
        {
            despachoNoFacturado.Activa = false;
            despachoNoFacturado.ValorUmbral = 0.0;
        }

        // La columna Ambito no existía antes; las filas anteriores a la migración pudieron quedar
        // sin un valor de enum válido (0). Se normalizan a Auditoría antes de marcar las Operativa.
        var ambitoInvalido = await context.ReglasDeteccion
            .Where(r => r.Ambito != AmbitoAlerta.Operativa && r.Ambito != AmbitoAlerta.Auditoria
                     && r.Ambito != AmbitoAlerta.Ambos)
            .ToListAsync();
        foreach (var r in ambitoInvalido) r.Ambito = AmbitoAlerta.Auditoria;

        // Carril correcto en bases ya sembradas (la columna Ambito no existía antes): estos tres son
        // problemas operativos de estación (errores honestos), no anomalías de auditoría.
        var clavesOperativas = new[]
        {
            "TurnoSinCerrarHorasUmbral", "DespachoNoFacturadoHabilitado", "CamposObligatoriosHabilitado"
        };
        var aOperativa = await context.ReglasDeteccion
            .Where(r => clavesOperativas.Contains(r.ParametroNombre) && r.Ambito != AmbitoAlerta.Operativa)
            .ToListAsync();
        foreach (var r in aOperativa) r.Ambito = AmbitoAlerta.Operativa;

        // Despachos rápidos: ámbito AMBOS (operativa + auditoría) en bases ya sembradas, solo si sigue en el
        // default anterior (Auditoría), para no pisar un cambio manual. La recalibración (acumulable, mínimo
        // 2, por RUC/placa) es de código; aquí solo se asegura el carril.
        var despachosRapidos = await context.ReglasDeteccion
            .FirstOrDefaultAsync(r => r.ParametroNombre == "DespachosRapidosMinutosUmbral");
        if (despachosRapidos is not null && despachosRapidos.Ambito == AmbitoAlerta.Auditoria)
            despachosRapidos.Ambito = AmbitoAlerta.Ambos;

        // Recalibrar la tasa de anulaciones del 5% al 3% (la tesis indica que lo normal es <2%),
        // solo si la regla sigue en el valor por defecto anterior (no piso ajustes manuales).
        var anulaciones = await context.ReglasDeteccion
            .FirstOrDefaultAsync(r => r.ParametroNombre == "AnulacionesPorcentajeUmbral");
        if (anulaciones is not null && Math.Abs(anulaciones.ValorUmbral - 5.0) < 0.001)
            anulaciones.ValorUmbral = 3.0;
    }

    /// <summary>
    /// Crea usuarios demo Auditor y Supervisor si no existen (para CU-11 y demostración de RBAC).
    /// </summary>
    private static async Task EnsureUsuariosDemoAsync(PetrolRiosDbContext context)
    {
        var roles = await context.Roles.ToDictionaryAsync(r => r.Nombre, r => r.Id);

        if (!await context.Usuarios.AnyAsync(u => u.Email == "auditor@petrolrios.com")
            && roles.TryGetValue("Auditor", out var auditorRolId))
        {
            var auditor = Usuario.Create(
                "auditor@petrolrios.com",
                "Maria Fernanda Auditora",
                BCrypt.Net.BCrypt.HashPassword("Auditor123!"),
                auditorRolId);
            auditor.MarcarEmailVerificado();
            await context.Usuarios.AddAsync(auditor);
        }

        if (!await context.Usuarios.AnyAsync(u => u.Email == "supervisor@petrolrios.com")
            && roles.TryGetValue("Supervisor", out var supervisorRolId))
        {
            var supervisor = Usuario.Create(
                "supervisor@petrolrios.com",
                "Carlos Supervisor de Auditoria",
                BCrypt.Net.BCrypt.HashPassword("Supervisor123!"),
                supervisorRolId);
            supervisor.MarcarEmailVerificado();
            await context.Usuarios.AddAsync(supervisor);
        }
    }

    private static async Task SeedRolesAsync(PetrolRiosDbContext context)
    {
        var roles = new[]
        {
            Rol.Create("Auditor", "Auditor interno - revisa alertas y documenta hallazgos"),
            Rol.Create("Supervisor", "Supervisor de auditoría - asigna casos, configura umbrales, genera reportes"),
            Rol.Create("Administrador", "Administrador del sistema - gestiona usuarios, roles y configuración"),
            Rol.Create("Agente", "Agente de estacion - cuenta de servicio del Station Agent: solo conecta y envia datos, SIN acceso a la app central")
        };
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsuarioAdminAsync(PetrolRiosDbContext context, IConfiguration config)
    {
        var adminRol = await context.Roles.FirstAsync(r => r.Nombre == "Administrador");

        // La contraseña inicial NO está quemada: se toma de configuración/variable de
        // entorno (Seguridad:AdminPasswordInicial). En desarrollo cae a un valor demo,
        // pero SIEMPRE se obliga a cambiarla en el primer ingreso.
        var passwordInicial = config["Seguridad:AdminPasswordInicial"];
        if (string.IsNullOrWhiteSpace(passwordInicial))
            passwordInicial = "Admin123!"; // solo demo/desarrollo

        var admin = Usuario.Create(
            "admin@petrolrios.com",
            "Administrador del Sistema",
            BCrypt.Net.BCrypt.HashPassword(passwordInicial),
            adminRol.Id);
        admin.DebeCambiarPassword = true;
        admin.MarcarEmailVerificado(); // cuenta del sistema
        await context.Usuarios.AddAsync(admin);
    }

    private static async Task SeedEstacionesAsync(PetrolRiosDbContext context)
    {
        var estaciones = new[]
        {
            Estacion.Create("Estacion Santo Domingo Centro", "EST-001", "Av. Quito y Av. Tsachila, Santo Domingo", "Centro"),
            Estacion.Create("Estacion Santo Domingo Norte", "EST-002", "Via Quito Km 1, Santo Domingo", "Norte"),
            Estacion.Create("Estacion Santo Domingo Sur", "EST-003", "Via Quevedo Km 2, Santo Domingo", "Sur"),
            Estacion.Create("Estacion La Concordia", "EST-004", "Via La Concordia, Km 35", "Norte"),
            Estacion.Create("Estacion Quininde", "EST-005", "Via Quininde, Km 20", "Norte"),
            Estacion.Create("Estacion El Carmen", "EST-006", "Via El Carmen, Km 30", "Centro"),
            Estacion.Create("Estacion Pedernales", "EST-007", "Via Pedernales, Km 80", "Sur"),
            Estacion.Create("Estacion Chone", "EST-008", "Av. Principal, Chone", "Sur"),
            Estacion.Create("Estacion Buena Fe", "EST-009", "Via Buena Fe, Km 45", "Centro"),
            Estacion.Create("Estacion Patricia Pilar", "EST-010", "Via Patricia Pilar, Km 50", "Norte")
        };
        await context.Estaciones.AddRangeAsync(estaciones);
        await context.SaveChangesAsync();

        // Watermarks iniciales (desde hace 1 hora para primera extracción)
        var watermarkInicial = DateTime.UtcNow.AddHours(-1);
        foreach (var est in estaciones)
        {
            await context.EstacionWatermarks.AddAsync(
                EstacionWatermark.Create(est.Id, watermarkInicial));
        }
    }

    private static async Task SeedReglasDeteccionAsync(PetrolRiosDbContext context)
    {
        // Operacion fuera de horario: DESHABILITADA por defecto porque las estaciones de
        // PetrolRios operan 24/7. Queda disponible para estaciones con horario restringido.
        var reglaFueraHorario = ReglaDeteccion.Create(
            TipoDetector.ComplianceViolation,
            "Operacion fuera de horario",
            "Genera alerta si se registran transacciones fuera del horario configurado por estacion. Deshabilitada por defecto: las estaciones operan 24/7",
            "FueraHorarioHabilitado",
            0.0);
        reglaFueraHorario.Activa = false;

        // Umbrales por defecto según tesis Tabla 3
        var reglas = new[]
        {
            // Cash Fraud
            ReglaDeteccion.Create(
                TipoDetector.CashFraud,
                "Diferencia efectivo vs sistema",
                "Genera alerta si la diferencia entre efectivo reportado y calculado por el sistema excede el umbral por turno",
                "DiferenciaEfectivoUmbral",
                50.0),
            ReglaDeteccion.Create(
                TipoDetector.CashFraud,
                "Patron de faltantes recurrentes (gineteo)",
                "Genera alerta si el mismo empleado tiene faltantes mayor al umbral de ocurrencias en los ultimos 30 dias",
                "FaltantesRecurrentesMaximo",
                3.0),
            ReglaDeteccion.Create(
                TipoDetector.CashFraud,
                "Periodo de evaluacion de faltantes",
                "Cantidad de dias hacia atras para evaluar patron de faltantes recurrentes",
                "FaltantesRecurrentesDias",
                30.0),
            ReglaDeteccion.Create(
                TipoDetector.CashFraud,
                "Venta a credito sin cliente identificado",
                "Genera alerta si una venta a credito no tiene cliente o identificacion registrada (posible venta en efectivo registrada como credito)",
                "CreditoSinClienteHabilitado",
                1.0),
            ReglaDeteccion.Create(
                TipoDetector.CashFraud,
                "Proporcion atipica de efectivo corporativo",
                "Genera alerta si un vendedor supera el porcentaje umbral de ventas en efectivo sobre clientes corporativos (patron del caso documentado de enero 2026)",
                "EfectivoCorporativoPorcentajeUmbral",
                30.0),

            // Invoice Anomaly
            ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Tasa de anulaciones excesivas",
                "Genera alerta si el porcentaje de anulaciones supera el umbral de las transacciones diarias (la tesis indica que lo normal es <2%)",
                "AnulacionesPorcentajeUmbral",
                3.0),
            ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Precio fuera de lista",
                "Genera alerta si el precio aplicado excede el precio autorizado en la lista de precios",
                "PrecioFueraListaHabilitado",
                1.0),
            ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Campos obligatorios vacios",
                "Genera alerta si la factura tiene campos obligatorios vacios (placa, identificacion) segun configuracion",
                "CamposObligatoriosHabilitado",
                1.0),
            ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Descuento excesivo fuera de politica",
                "Genera alerta si el descuento aplicado excede el porcentaje maximo permitido por la politica comercial",
                "DescuentoPorcentajeMaximo",
                10.0),
            ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly,
                "Total de factura inconsistente",
                "Genera alerta si el total registrado no corresponde a subtotal - descuento + IVA (indicador de manipulacion documental)",
                "TotalInconsistenteHabilitado",
                1.0),

            // Payment Fraud
            ReglaDeteccion.Create(
                TipoDetector.PaymentFraud,
                "Reversion tarjeta tardia",
                "Genera alerta si una reversion de tarjeta ocurre mas de N minutos despues de la venta original",
                "ReversionTarjetaMinutosUmbral",
                30.0),
            ReglaDeteccion.Create(
                TipoDetector.PaymentFraud,
                "Credito sin autorizacion",
                "Genera alerta si un credito otorgado excede el limite del cliente sin codigo de autorizacion",
                "CreditoSinAutorizacionHabilitado",
                1.0),
            ReglaDeteccion.Create(
                TipoDetector.PaymentFraud,
                "Transacciones duplicadas",
                "Genera alerta si se detectan transacciones con misma tarjeta, mismo monto y diferencia menor a N minutos",
                "DuplicadaMinutosUmbral",
                5.0),
            ReglaDeteccion.Create(
                TipoDetector.PaymentFraud,
                "Despachos rapidos sucesivos",
                "Genera alerta si el mismo RUC/cliente (o la misma placa) registra 2 o mas despachos consecutivos con menos de N minutos entre ellos. Dinamica (RUC o placa) y acumulable: se acumulan en UNA alerta por caso que escala por cantidad (2-3 Medio, 4-5 Alto, 6+ Critico) y re-emerge arriba. Ambito Ambos.",
                "DespachosRapidosMinutosUmbral",
                10.0, AmbitoAlerta.Ambos),

            // Compliance Violation
            ReglaDeteccion.Create(
                TipoDetector.ComplianceViolation,
                "Venta excesiva a placa generica",
                "Genera alerta si placa ZZZ999949 tiene venta mayor a N galones (regulacion ARCERNNR)",
                "PlacaGenericaGalonesMaximo",
                5.0),
            ReglaDeteccion.Create(
                TipoDetector.ComplianceViolation,
                "Vehiculo con multiples combustibles",
                "Genera alerta si la misma placa tiene ventas de diesel y gasolina extra en el mismo dia",
                "MultipleCombustibleHabilitado",
                1.0),
            ReglaDeteccion.Create(
                TipoDetector.ComplianceViolation,
                "Venta sin placa en monto mayor",
                "Genera alerta si una venta supera el monto umbral sin placa registrada (trazabilidad exigida por normativa de comercializacion)",
                "VentaSinPlacaMontoMinimo",
                200.0),
            reglaFueraHorario
        };
        await context.ReglasDeteccion.AddRangeAsync(reglas);
    }

    private static async Task SeedAgentUsersAsync(PetrolRiosDbContext context)
    {
        // Cuentas de servicio tipo Agente para los Station Agents (1 por estacion).
        // Rol "Agente": solo ingesta/heartbeat; SIN acceso a la app central (lo bloquea la politica Central).
        var agenteRol = await context.Roles.FirstAsync(r => r.Nombre == "Agente");
        var estaciones = await context.Estaciones.ToListAsync();

        foreach (var est in estaciones)
        {
            var email = $"agent-{est.Codigo.ToLower()}@petrolrios.com";
            var agent = Usuario.Create(
                email,
                $"Agente Estacion {est.Codigo}",
                BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                agenteRol.Id);
            agent.AsignarEstacion(est.Id);
            agent.MarcarEmailVerificado(); // cuenta de servicio del agente
            await context.Usuarios.AddAsync(agent);
        }
    }
}
