using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class LateReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateTime returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);

            // days
            var rentedDays = (returnDate - rental.StartDate).Days;
            var lateDays = (returnDate - rental.EndDate).Days;

            // values
            var totalCost = rentedDays * dailyRate + lateDays * 50;

            return totalCost;
        }
    }
}
