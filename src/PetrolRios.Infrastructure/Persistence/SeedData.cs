using Microsoft.EntityFrameworkCore;
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

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            await SeedRolesAsync(context);
            await SeedUsuarioAdminAsync(context);
            await SeedEstacionesAsync(context);
            await SeedReglasDeteccionAsync(context);
            await SeedAgentUsersAsync(context);

            await context.SaveChangesAsync();
        }

        // Pasos idempotentes: aplican también sobre bases ya sembradas
        await EnsureReglasNuevasAsync(context);
        await EnsureUsuariosDemoAsync(context);

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

        void AddIfMissing(TipoDetector tipo, string nombre, string descripcion, string parametro, double umbral)
        {
            if (!existentes.Contains(parametro))
                nuevas.Add(ReglaDeteccion.Create(tipo, nombre, descripcion, parametro, umbral));
        }

        AddIfMissing(TipoDetector.CashFraud,
            "Venta a credito sin cliente identificado",
            "Genera alerta si una venta a credito no tiene cliente o identificacion registrada (posible venta en efectivo registrada como credito)",
            "CreditoSinClienteHabilitado", 1.0);
        AddIfMissing(TipoDetector.CashFraud,
            "Proporcion atipica de efectivo corporativo",
            "Genera alerta si un vendedor supera el porcentaje umbral de ventas en efectivo sobre clientes corporativos (patron del caso documentado de enero 2026)",
            "EfectivoCorporativoPorcentajeUmbral", 30.0);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Descuento excesivo fuera de politica",
            "Genera alerta si el descuento aplicado excede el porcentaje maximo permitido por la politica comercial",
            "DescuentoPorcentajeMaximo", 10.0);
        AddIfMissing(TipoDetector.InvoiceAnomaly,
            "Total de factura inconsistente",
            "Genera alerta si el total registrado no corresponde a subtotal - descuento + IVA (indicador de manipulacion documental)",
            "TotalInconsistenteHabilitado", 1.0);
        AddIfMissing(TipoDetector.PaymentFraud,
            "Despachos rapidos sucesivos",
            "Genera alerta si el mismo cliente registra 3 o mas despachos consecutivos con menos de N minutos entre ellos (patron del caso documentado de enero 2026)",
            "DespachosRapidosMinutosUmbral", 10.0);
        AddIfMissing(TipoDetector.ComplianceViolation,
            "Venta sin placa en monto mayor",
            "Genera alerta si una venta supera el monto umbral sin placa registrada (trazabilidad exigida por normativa de comercializacion)",
            "VentaSinPlacaMontoMinimo", 200.0);

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
            await context.Usuarios.AddAsync(Usuario.Create(
                "auditor@petrolrios.com",
                "Maria Fernanda Auditora",
                BCrypt.Net.BCrypt.HashPassword("Auditor123!"),
                auditorRolId));
        }

        if (!await context.Usuarios.AnyAsync(u => u.Email == "supervisor@petrolrios.com")
            && roles.TryGetValue("Supervisor", out var supervisorRolId))
        {
            await context.Usuarios.AddAsync(Usuario.Create(
                "supervisor@petrolrios.com",
                "Carlos Supervisor de Auditoria",
                BCrypt.Net.BCrypt.HashPassword("Supervisor123!"),
                supervisorRolId));
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

    private static async Task SeedUsuarioAdminAsync(PetrolRiosDbContext context)
    {
        var adminRol = await context.Roles.FirstAsync(r => r.Nombre == "Administrador");
        // BCrypt hash de "Admin123!"
        var admin = Usuario.Create(
            "admin@petrolrios.com",
            "Administrador del Sistema",
            BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            adminRol.Id);
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
                "Genera alerta si el porcentaje de anulaciones del empleado supera el umbral de sus transacciones diarias",
                "AnulacionesPorcentajeUmbral",
                5.0),
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
            await context.Usuarios.AddAsync(agent);
        }
    }
}
