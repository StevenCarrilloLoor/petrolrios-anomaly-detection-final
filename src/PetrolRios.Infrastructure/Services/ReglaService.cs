using PetrolRios.Application.DTOs.Reglas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Services;

public sealed class ReglaService : IReglaService
{
    private readonly IUnitOfWork _unitOfWork;

    // Reglas del motor (IDetectionRule): traen los valores predeterminados de fábrica (umbral y carril)
    // como fuente única de verdad, para poder restablecer una regla a su default sin duplicar valores.
    private readonly IReadOnlyList<IDetectionRule> _reglasMotor;

    public ReglaService(IUnitOfWork unitOfWork, IEnumerable<IDetectionRule> reglasMotor)
    {
        _unitOfWork = unitOfWork;
        _reglasMotor = reglasMotor as IReadOnlyList<IDetectionRule> ?? reglasMotor.ToList();
    }

    public async Task<IReadOnlyList<ReglaDeteccionResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var reglas = await _unitOfWork.ReglasDeteccion.GetAllAsync(ct);
        return reglas.Select(MapToResponse).ToList();
    }

    public async Task<ReglaDeteccionResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var regla = await _unitOfWork.ReglasDeteccion.GetByIdAsync(id, ct);
        return regla is null ? null : MapToResponse(regla);
    }

    public async Task<ReglaDeteccionResponse> CreateAsync(CrearReglaRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<TipoDetector>(request.TipoDetector, true, out var tipo))
            throw new ArgumentException($"TipoDetector '{request.TipoDetector}' no es válido.");

        var regla = ReglaDeteccion.Create(tipo, request.Nombre, request.Descripcion,
            request.ParametroNombre, request.ValorUmbral);

        await _unitOfWork.ReglasDeteccion.AddAsync(regla, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(regla);
    }

    public async Task<ReglaDeteccionResponse> UpdateAsync(int id, ActualizarReglaRequest request, CancellationToken ct = default)
    {
        var regla = await _unitOfWork.ReglasDeteccion.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Regla {id} no encontrada.");

        if (request.ValorUmbral.HasValue)
            regla.ValorUmbral = request.ValorUmbral.Value;
        if (request.Activa.HasValue)
            regla.Activa = request.Activa.Value;
        if (!string.IsNullOrWhiteSpace(request.Ambito)
            && Enum.TryParse<AmbitoAlerta>(request.Ambito, true, out var ambito))
            regla.Ambito = ambito;
        if (request.NotificarCorreo.HasValue)
            regla.NotificarCorreo = request.NotificarCorreo.Value;

        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(regla);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var regla = await _unitOfWork.ReglasDeteccion.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Regla {id} no encontrada.");

        _unitOfWork.ReglasDeteccion.Remove(regla);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Defaults de parámetros que no son una IDetectionRule propia (sub-parámetro de otra regla).</summary>
    private static readonly Dictionary<string, double> UmbralesHuerfanos = new()
    {
        ["FaltantesRecurrentesDias"] = 30.0,
    };

    public async Task<IReadOnlyList<ReglaDeteccionResponse>> RestablecerDetectorAsync(
        string tipoDetector, CancellationToken ct = default)
    {
        if (!Enum.TryParse<TipoDetector>(tipoDetector, true, out var tipo))
            throw new ArgumentException($"TipoDetector '{tipoDetector}' no es válido.");

        var todas = await _unitOfWork.ReglasDeteccion.GetAllAsync(ct);
        var ids = todas.Where(r => r.TipoDetector == tipo).Select(r => r.Id).ToList();

        var actualizadas = new List<ReglaDeteccion>();
        foreach (var id in ids)
        {
            // GetByIdAsync devuelve la entidad rastreada (igual que UpdateAsync), para que persista.
            var regla = await _unitOfWork.ReglasDeteccion.GetByIdAsync(id, ct);
            if (regla is null) continue;

            var motor = _reglasMotor.FirstOrDefault(m => m.Parametro == regla.ParametroNombre);
            if (motor is not null)
            {
                regla.ValorUmbral = motor.UmbralPorDefecto;
                regla.Ambito = motor.AmbitoPorDefecto;
            }
            else if (UmbralesHuerfanos.TryGetValue(regla.ParametroNombre, out var umbral))
            {
                regla.ValorUmbral = umbral;
            }

            // De fábrica todas están activas salvo "fuera de horario" (estaciones 24/7) y sin aviso por correo.
            regla.Activa = regla.ParametroNombre != "FueraHorarioHabilitado";
            regla.NotificarCorreo = false;
            actualizadas.Add(regla);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return actualizadas.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Unidad + ayuda del umbral por parámetro: para que el editor muestre qué significa cada número
    /// (antes solo se veía "umbral 18" sin saber si eran horas, $, % o galones). Los parámetros del
    /// motor son fijos, así que es metadato en código (no necesita columna en BD).
    /// </summary>
    private static readonly Dictionary<string, (string Unidad, string Ayuda)> UmbralMeta = new()
    {
        ["DiferenciaEfectivoUmbral"] = ("USD ($)", "Diferencia en dólares entre el efectivo reportado y el calculado por el sistema, por turno."),
        ["FaltantesRecurrentesMaximo"] = ("veces", "Número de faltantes del mismo empleado en el período para considerarlo patrón (gineteo)."),
        ["FaltantesRecurrentesDias"] = ("días", "Días hacia atrás para evaluar el patrón de faltantes recurrentes."),
        ["CreditoSinClienteHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["EfectivoCorporativoPorcentajeUmbral"] = ("%", "Porcentaje de ventas en efectivo sobre clientes corporativos que dispara la alerta."),
        ["AnulacionesPorcentajeUmbral"] = ("%", "Porcentaje de anulaciones sobre las transacciones del día (lo normal es <2%)."),
        ["PrecioFueraListaHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["CamposObligatoriosHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["DescuentoPorcentajeMaximo"] = ("%", "Porcentaje máximo de descuento permitido por la política comercial."),
        ["TotalInconsistenteHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["ReversionTarjetaMinutosUmbral"] = ("minutos", "Minutos tras la venta a partir de los cuales una reversión de tarjeta es sospechosa."),
        ["CreditoSinAutorizacionHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["DuplicadaMinutosUmbral"] = ("minutos", "Ventana en minutos para considerar dos cobros (misma tarjeta y monto) como duplicados."),
        ["DespachosRapidosMinutosUmbral"] = ("minutos", "Minutos máximos entre despachos consecutivos del mismo cliente para marcarlos como sospechosos."),
        ["PlacaGenericaGalonesMaximo"] = ("galones", "Galones máximos permitidos a la placa genérica ZZZ999949 (regulación ARCERNNR)."),
        ["MultipleCombustibleHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["VentaSinPlacaMontoMinimo"] = ("USD ($)", "Monto en dólares a partir del cual se exige placa registrada (trazabilidad)."),
        ["TurnoSinCerrarHorasUmbral"] = ("horas", "Horas que un turno puede seguir abierto antes de generar el aviso operativo."),
        ["DespachoNoFacturadoHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
        ["FueraHorarioHabilitado"] = ("1 = activado", "Interruptor: 1 activa la regla, 0 la desactiva."),
    };

    private static ReglaDeteccionResponse MapToResponse(ReglaDeteccion r)
    {
        var (unidad, ayuda) = UmbralMeta.TryGetValue(r.ParametroNombre, out var m) ? m : ("valor", "");
        return new()
        {
            Id = r.Id,
            TipoDetector = r.TipoDetector.ToString(),
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            ParametroNombre = r.ParametroNombre,
            ValorUmbral = r.ValorUmbral,
            Unidad = unidad,
            AyudaUmbral = ayuda,
            Activa = r.Activa,
            Ambito = r.Ambito.ToString(),
            NotificarCorreo = r.NotificarCorreo
        };
    }
}
