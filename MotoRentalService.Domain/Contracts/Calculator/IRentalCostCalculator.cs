using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Services.Interfaces
{
    public interface IRentalCostCalculator
    {
        decimal Calculate(Rental rental, DateOnly returnDate);
    }
}
