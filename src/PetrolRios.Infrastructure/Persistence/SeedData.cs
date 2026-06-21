using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        await EnsureUsuariosDemoAsync(context);
        await EnsureAgentUsersStationAssignmentAsync(context);
        await EnsureCuentasAccesoAsync(context, config);

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
            "Genera una alerta operativa si un despacho (DESP) con galones servidos no esta marcado como facturado (FAC_DESP); combustible que salio sin cobrarse.",
            "DespachoNoFacturadoHabilitado", 1.0, AmbitoAlerta.Operativa);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Anulaciones recurrentes (kiting)",
            "Genera alerta si un mismo punto de emision tiene anulaciones en varios dias distintos (umbral = dias minimos); posible patron de cancelar y reingresar para rodar la deuda o mover el periodo.",
            "AnulacionRecurrenteDiasMinimo", 3.0);
        AddIfMissing(TipoDetector.PaymentFraud,
            "Despachos rapidos sucesivos",
            "Genera alerta si el mismo cliente registra 3 o mas despachos consecutivos con menos de N minutos entre ellos (patron del caso documentado de enero 2026)",
            "DespachosRapidosMinutosUmbral", 10.0);
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

        // La columna Ambito no existía antes; las filas anteriores a la migración pudieron quedar
        // sin un valor de enum válido (0). Se normalizan a Auditoría antes de marcar las Operativa.
        var ambitoInvalido = await context.ReglasDeteccion
            .Where(r => r.Ambito != AmbitoAlerta.Operativa && r.Ambito != AmbitoAlerta.Auditoria)
            .ToListAsync();
        foreach (var r in ambitoInvalido) r.Ambito = AmbitoAlerta.Auditoria;

        // Carril correcto en bases ya sembradas (la columna Ambito no existía antes): estos tres son
        // problemas operativos de estación (errores honestos), no fraude.
        var clavesOperativas = new[]
        {
            "TurnoSinCerrarHorasUmbral", "DespachoNoFacturadoHabilitado", "CamposObligatoriosHabilitado"
        };
        var aOperativa = await context.ReglasDeteccion
            .Where(r => clavesOperativas.Contains(r.ParametroNombre) && r.Ambito != AmbitoAlerta.Operativa)
            .ToListAsync();
        foreach (var r in aOperativa) r.Ambito = AmbitoAlerta.Operativa;

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
            Rol.Create("Administrador", "Administrador del sistema - gestiona usuarios, roles y configuración")
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
                "Genera alerta si el mismo cliente registra 3 o mas despachos consecutivos con menos de N minutos entre ellos (patron del caso documentado de enero 2026)",
                "DespachosRapidosMinutosUmbral",
                10.0),

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
        // Usuarios tipo Auditor para los Station Agents (1 por estacion)
        var auditorRol = await context.Roles.FirstAsync(r => r.Nombre == "Auditor");
        var estaciones = await context.Estaciones.ToListAsync();

        foreach (var est in estaciones)
        {
            var email = $"agent-{est.Codigo.ToLower()}@petrolrios.com";
            var agent = Usuario.Create(
                email,
                $"Agente Estacion {est.Codigo}",
                BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                auditorRol.Id);
            agent.AsignarEstacion(est.Id);
            agent.MarcarEmailVerificado(); // cuenta de servicio del agente
            await context.Usuarios.AddAsync(agent);
        }
    }
}
