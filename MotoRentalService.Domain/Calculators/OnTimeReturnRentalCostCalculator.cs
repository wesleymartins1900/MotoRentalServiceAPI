using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class OnTimeReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateOnly returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);
            
            var rentedDays = (returnDate.ToDateTime(TimeOnly.MinValue) - rental.StartDate.ToDateTime(TimeOnly.MinValue)).Days;
            var totalCost = rentedDays * dailyRate;

            return totalCost;
        }
    }
}
