using FluentValidation;
using MotoRentalService.Application.Dtos;

namespace MotoRentalService.Application.Validators.DeliveryPerson
{
    public class RegisterDeliveryPersonDtoValidator : AbstractValidator<RegisterDeliveryPersonDto>
    {
        public RegisterDeliveryPersonDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("BirthDate is required.")
                 .Must(BeAtLeast18YearsOld).WithMessage("You must be at least 18 years old.");

            RuleFor(x => x.Cnpj)
                .NotEmpty().WithMessage("CNPJ is required.")
                .Matches(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$|^\d{14}$")
                .WithMessage("CNPJ must be either 14 digits if formatted or 18 characters if unformatted (including dots, slashes, and hyphens)..");

            RuleFor(x => x.CnhNumber)
                .NotEmpty().WithMessage("CNH Number is required.")
                .Matches(@"^\d{11}$").WithMessage("CNH Number must be 11 digits.");

            RuleFor(x => x.CnhType)
                .NotEmpty().WithMessage("CNH Type is required.");
        }

        private bool BeAtLeast18YearsOld(DateOnly birthDate)
        {
            var birthDateTime = birthDate.ToDateTime(TimeOnly.MinValue);
            var currentDateTime = DateTime.Now;

            var calculatedAge = currentDateTime.Year - birthDateTime.Year;

            if (currentDateTime > birthDateTime.AddYears(-calculatedAge))
                calculatedAge--;

            return calculatedAge >= 18;
        }
    }
}
