using FluentValidation;
using MotoRentalService.Application.Dtos;

namespace MotoRentalService.Application.Validators
{
    public class RentalMotoDtoValidator : AbstractValidator<RentalMotoDto>
    {
        public RentalMotoDtoValidator()
        {
            RuleFor(r => r.DeliveryPersonId)
                .NotEmpty().WithMessage("Delivery person ID is required.");

            RuleFor(r => r.MotoId)
                .NotEmpty().WithMessage("Moto ID is required.");

            //RuleFor(r => r.EndDate)
            //    .GreaterThanOrEqualTo(r => r.ExpectedEndDate)
            //    .WithMessage("End date must be greater than or equal to the expected end date.");
        }
    }
}
