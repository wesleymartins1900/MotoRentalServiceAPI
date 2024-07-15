using MotoRentalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MotoRentalService.Domain.Contracts.Repositories;

namespace MotoRentalService.Infrastructure.Data.Repositories
{
    public class RentalRepository(MotoRentalDbContext dbContext) : IRentalRepository
    {
        private readonly MotoRentalDbContext _dbContext = dbContext;

        /// <summary>
        /// Retrieves a <see cref="Rental"/> by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the rental.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the rental if found; otherwise, <c>null</c>.</returns>
        public async Task<Rental?> GetByIdAsync(Guid id) => await _dbContext.Rentals.FindAsync(id);

        /// <summary>
        /// Adds a new <see cref="Rental"/> to the database.
        /// </summary>
        /// <param name="rental">The rental to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(Rental rental)
        {
            await _dbContext.Rentals.AddAsync(rental);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing <see cref="Rental"/> in the database.
        /// </summary>
        /// <param name="rental">The rental with updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(Rental rental)
        {
            _dbContext.Rentals.Update(rental);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a <see cref="Rental"/> from the database by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the rental to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RemoveAsync(Guid id)
        {
            var rental = await GetByIdAsync(id);
            if (rental != null)
            {
                _dbContext.Rentals.Remove(rental);
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if there are any rentals associated with the specified motorcycle ID.
        /// </summary>
        /// <param name="motoId">The ID of the motorcycle to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if rentals exist; otherwise, <c>false</c>.</returns>
        public async Task<bool> ExistsRentalsByMotoIdAsync(Guid motoId) => await _dbContext.Rentals.Where(x => x.MotoId == motoId).AnyAsync();
    }
}
