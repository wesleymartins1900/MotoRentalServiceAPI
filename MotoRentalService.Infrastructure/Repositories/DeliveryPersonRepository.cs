using Microsoft.EntityFrameworkCore;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Infrastructure.Data;

namespace MotoRentalService.Infrastructure.Repositories
{
    public class DeliveryPersonRepository(MotoRentalDbContext context) : IDeliveryPersonRepository
    {
        private readonly MotoRentalDbContext _context = context;

        /// <summary>
        /// Adds a new <see cref="DeliveryPerson"/> to the database.
        /// </summary>
        /// <param name="deliveryPerson">The delivery person to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddAsync(DeliveryPerson deliveryPerson)
        {
            await _context.DeliveryPeople.AddAsync(deliveryPerson);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing <see cref="DeliveryPerson"/> in the database.
        /// </summary>
        /// <param name="deliveryPerson">The delivery person with updated information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(DeliveryPerson deliveryPerson)
        {
            _context.DeliveryPeople.Update(deliveryPerson);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a <see cref="DeliveryPerson"/> by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the delivery person.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the delivery person if found; otherwise, <c>null</c>.</returns>
        public async Task<DeliveryPerson?> GetByIdAsync(Guid id) => await _context.DeliveryPeople.AsNoTracking().FirstOrDefaultAsync(dp => dp.Id.Equals(id));

        /// <summary>
        /// Retrieves a <see cref="DeliveryPerson"/> by its CNPJ.
        /// </summary>
        /// <param name="cnpj">The CNPJ of the delivery person.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the delivery person if found; otherwise, <c>null</c>.</returns>
        public async Task<DeliveryPerson?> GetByCnpjAsync(string cnpj) => await _context.DeliveryPeople.AsNoTracking().FirstOrDefaultAsync(dp => dp.Cnpj.Value == cnpj);

        /// <summary>
        /// Retrieves a <see cref="DeliveryPerson"/> by its CNH number.
        /// </summary>
        /// <param name="cnhNumber">The CNH number of the delivery person.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the delivery person if found; otherwise, <c>null</c>.</returns>
        public async Task<DeliveryPerson?> GetByCnhNumberAsync(string cnhNumber) => await _context.DeliveryPeople.AsNoTracking().FirstOrDefaultAsync(dp => dp.Cnh.Number == cnhNumber);
    }
}
