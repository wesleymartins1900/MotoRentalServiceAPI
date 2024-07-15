using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Services.Interfaces
{
    public interface IRentalCostCalculator
    {
        Task<Result<decimal>> CalculateAsync(Rental rental, DateTime returnDate);
    }
}
