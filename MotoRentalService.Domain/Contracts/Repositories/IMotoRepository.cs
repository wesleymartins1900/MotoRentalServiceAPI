using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Shared;

namespace MotoRentalService.Domain.Contracts.Repositories
{
    public interface IMotoRepository
    {
        Task<Moto?> GetByIdAsync(Guid id);
        Task<Moto?> GetByPlateAsync(string plateFilter);
        Task<PagedResult<Moto>> GetMotosAsync(string? plate, int pageNumber, int pageSize);
        Task AddAsync(Moto moto);
        Task UpdateAsync(Moto moto);
        Task RemoveAsync(Guid id);
    }
}
