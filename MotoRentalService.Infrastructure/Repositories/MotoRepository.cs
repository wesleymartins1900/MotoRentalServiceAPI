using MotoRentalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MotoRentalService.Domain.Shared;
using MotoRentalService.Domain.Contracts.Repositories;

namespace MotoRentalService.Infrastructure.Data.Repositories
{
    public class MotoRepository(MotoRentalDbContext dbContext) : IMotoRepository
    {
        private readonly MotoRentalDbContext _dbContext = dbContext;

        /// <summary>
        /// Retrieves a <see cref="Moto"/> by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the moto.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the moto if found and not deleted; otherwise, <c>null</c>.</returns>
        public async Task<Moto?> GetByIdAsync(Guid id) => await _dbContext.Motos.Where(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();

        /// <summary>
        /// Adds a new <see cref="Moto"/> to the database.
        /// </summary>
        /// <param name="moto">The moto to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(Moto moto)
        {
            await _dbContext.Motos.AddAsync(moto);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing <see cref="Moto"/> in the database.
        /// </summary>
        /// <param name="moto">The moto with updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(Moto moto)
        {
            _dbContext.Motos.Update(moto);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a <see cref="Moto"/> from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the moto to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RemoveAsync(Guid id)
        {
            var moto = await GetByIdAsync(id);
            if (moto is null) return;

            _dbContext.Motos.Remove(moto);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a <see cref="Moto"/> by its plate number.
        /// </summary>
        /// <param name="plateFilter">The plate number filter.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the moto if found and not deleted; otherwise, <c>null</c>.</returns>

        public async Task<Moto?> GetByPlateAsync(string plateFilter) => await _dbContext.Motos
                                                                                       .Where(m => m.Plate.Contains(plateFilter)
                                                                                               && !m.Deleted)
                                                                                       .FirstOrDefaultAsync();

        /// <summary>
        /// Retrieves a paginated list of <see cref="Moto"/>s based on the plate number filter.
        /// </summary>
        /// <param name="plate">The plate number filter.</param>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PagedResult{Moto}"/> with the paginated motos.</returns>
        public async Task<PagedResult<Moto>> GetMotosAsync(string? plate, int pageNumber, int pageSize)
        {
            var query = _dbContext.Motos.AsQueryable();

            if (!string.IsNullOrEmpty(plate))
                query = query.Where(m => m.Plate.Contains(plate) && !m.Deleted);

            var totalCount = await query.CountAsync();

            var motos = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<Moto>
            {
                Items = motos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}