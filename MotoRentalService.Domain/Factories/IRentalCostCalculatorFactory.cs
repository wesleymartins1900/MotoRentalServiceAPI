using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Domain.Factories
{
    public interface IRentalCostCalculatorFactory
    {
        IRentalCostCalculator ChooseCalculatorBasedOnReturnDate(Rental rental, DateOnly returnDate);
    }
}
