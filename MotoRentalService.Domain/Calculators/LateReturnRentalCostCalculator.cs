using MotoRentalService.Domain.Entities;
using YourProject.Domain.Calculators;

namespace MotoRentalService.Domain.Calculator
{
    public class LateReturnRentalCostCalculator : BaseRentalCostCalculator
    {
        public override decimal Calculate(Rental rental, DateOnly returnDate)
        {
            var dailyRate = GetDailyRate(rental.PlanType);

            // days
            var rentedDays = (returnDate.ToDateTime(TimeOnly.MinValue) - rental.StartDate.ToDateTime(TimeOnly.MinValue)).Days;
            var lateDays = (returnDate.ToDateTime(TimeOnly.MinValue) - rental.EndDate.ToDateTime(TimeOnly.MinValue)).Days;

            // values
            var totalCost = rentedDays * dailyRate + lateDays * 50;

            return totalCost;
        }
    }
}
