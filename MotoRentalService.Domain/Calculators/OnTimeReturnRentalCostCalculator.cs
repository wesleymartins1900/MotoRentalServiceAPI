using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class OnTimeReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateTime returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);
            
            var daysRented = (returnDate - rental.StartDate).Days;
            var totalCost = daysRented * dailyRate;

            return totalCost;
        }
    }
}
