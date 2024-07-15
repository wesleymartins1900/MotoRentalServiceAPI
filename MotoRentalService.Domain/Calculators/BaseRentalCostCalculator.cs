using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Enums;

namespace YourProject.Domain.Calculators
{
    public abstract class BaseRentalCostCalculator : IRentalCostCalculator
    {
        public abstract decimal Calculate(Rental rental, DateTime returnDate);

        protected decimal GetDailyRate(RentalPlanType planType)
            => planType switch
            {
                RentalPlanType.SevenDays => 30,
                RentalPlanType.FifteenDays => 28,
                RentalPlanType.ThirtyDays => 22,
                RentalPlanType.FortyFiveDays => 20,
                RentalPlanType.FiftyDays => 18,
                _ => throw new ArgumentOutOfRangeException()
            };

        protected decimal GetPenaltyRate(RentalPlanType planType)
            => planType switch
            {
                RentalPlanType.SevenDays => 0.2M,
                RentalPlanType.FifteenDays => 0.4M,
                _ => 0
            };
    }
}
