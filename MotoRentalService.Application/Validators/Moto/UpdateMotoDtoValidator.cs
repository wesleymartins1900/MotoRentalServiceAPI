using FluentValidation;
using MotoRentalService.Application.Dtos;

namespace MotoRentalService.Application.Validators.Moto
{
    public class UpdateMotoDtoValidator : AbstractValidator<UpdateMotoDto>
    {
        public UpdateMotoDtoValidator()
        {
            RuleFor(moto => moto.Plate)
                .NotEmpty().WithMessage("The plate cannot be empty.")
                .Length(7).WithMessage("The plate must be 7 characters long.")
                .Matches("^[A-Z0-9]+$").WithMessage("The plate must consist of uppercase letters and digits only.");
        }
    }
}
