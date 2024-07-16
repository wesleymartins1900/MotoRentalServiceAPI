using MotoRentalService.Application.Dtos;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Services.Interfaces
{
    public interface IRentalService
    {
        Task<bool> ExistsRentalsByMotoIdAsync(Guid MotoId);
        Task<Result<Rental>> RegisterRentalMotoAsync(RentalMotoDto rentalMotoDto);
        Task<Result<decimal>> RentalValueCalculateAsync(Guid id, DateOnly endDate);
    }
}
