using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class EarlyReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateTime returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);
            var penaltyRate = GetPenaltyRate(rental.PlanType);

            // days
            var rentedDays = (returnDate - rental.StartDate).Days;
            var earlyDays = rental.EndDate.Day - returnDate.Day;

            // values
            var dailysValue = rentedDays * dailyRate;
            var penaltyDailyRate = earlyDays * (dailyRate * penaltyRate);
            
            return dailysValue + penaltyDailyRate;
        }
    }
}
