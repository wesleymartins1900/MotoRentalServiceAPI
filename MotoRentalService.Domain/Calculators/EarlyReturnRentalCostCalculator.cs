using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class EarlyReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateOnly returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);
            var penaltyRate = GetPenaltyRate(rental.PlanType);

            // days
            var rentedDays = (returnDate.ToDateTime(TimeOnly.MinValue) - rental.StartDate.ToDateTime(TimeOnly.MinValue)).Days;
            var earlyDays = (rental.EndDate.ToDateTime(TimeOnly.MinValue) - returnDate.ToDateTime(TimeOnly.MinValue)).Days;

            // values
            var dailysValue = rentedDays * dailyRate;
            var penaltyDailyRate = earlyDays * (dailyRate * penaltyRate);
            
            return dailysValue + penaltyDailyRate;
        }
    }
}
