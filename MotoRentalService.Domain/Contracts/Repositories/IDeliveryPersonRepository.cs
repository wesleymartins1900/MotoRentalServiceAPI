using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Domain.Contracts.Repositories
{
    public interface IDeliveryPersonRepository
    {
        Task<DeliveryPerson?> GetByIdAsync(Guid id);
        Task<DeliveryPerson?> GetByCnpjAsync(string cnpj);
        Task<DeliveryPerson?> GetByCnhNumberAsync(string cnhNumber);
        Task AddAsync(DeliveryPerson entregador);
        Task UpdateAsync(DeliveryPerson entregador);
    }
}
