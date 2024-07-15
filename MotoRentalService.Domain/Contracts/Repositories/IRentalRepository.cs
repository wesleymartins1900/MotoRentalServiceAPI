using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Domain.Contracts.Repositories
{
    public interface IRentalRepository
    {
        Task AddAsync(Rental rental);
        Task<Rental?> GetByIdAsync(Guid rentalId);
        Task<bool> ExistsRentalsByMotoIdAsync(Guid motoId);
        Task UpdateAsync(Rental rental);
    }
}
