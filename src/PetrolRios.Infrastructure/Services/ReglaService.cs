using PetrolRios.Application.DTOs.Reglas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Services;

public sealed class ReglaService : IReglaService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReglaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    private static ReglaDeteccionResponse MapToResponse(ReglaDeteccion r) => new()
    {
        Id = r.Id,
        TipoDetector = r.TipoDetector.ToString(),
        Nombre = r.Nombre,
        Descripcion = r.Descripcion,
        ParametroNombre = r.ParametroNombre,
        ValorUmbral = r.ValorUmbral,
        Activa = r.Activa,
        Ambito = r.Ambito.ToString()
    };
}
