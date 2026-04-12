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

        if (await context.Roles.AnyAsync())
            return; // Ya tiene datos

        await SeedRolesAsync(context);
        await SeedUsuarioAdminAsync(context);
        await SeedEstacionesAsync(context);
        await SeedReglasDeteccionAsync(context);

        await context.SaveChangesAsync();
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
                "Operacion fuera de horario",
                "Genera alerta si se registran transacciones fuera del horario configurado por estacion",
                "FueraHorarioHabilitado",
                1.0)
        };
        await context.ReglasDeteccion.AddRangeAsync(reglas);
    }
}
