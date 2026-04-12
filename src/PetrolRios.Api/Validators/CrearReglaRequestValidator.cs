using FluentValidation;
using PetrolRios.Application.DTOs.Reglas;

namespace PetrolRios.Api.Validators;

public sealed class CrearReglaRequestValidator : AbstractValidator<CrearReglaRequest>
{
    public CrearReglaRequestValidator()
    {
        RuleFor(x => x.TipoDetector)
            .NotEmpty().WithMessage("El tipo de detector es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.");

        RuleFor(x => x.ParametroNombre)
            .NotEmpty().WithMessage("El nombre del parámetro es obligatorio.");

        RuleFor(x => x.ValorUmbral)
            .GreaterThanOrEqualTo(0).WithMessage("El valor umbral debe ser mayor o igual a 0.");
    }
}
