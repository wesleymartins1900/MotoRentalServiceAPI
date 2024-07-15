using MotoRentalService.Application.Dtos;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Services.Interfaces
{
    public interface IDeliveryPersonService
    {
        Task<Result<DeliveryPerson>> RegisterDeliveryPersonAsync(RegisterDeliveryPersonDto deliveryPersonDto, ImageFromFormFileDto cnhImage);
        Task<Result> UpdateDeliveryPersonAsync(Guid id, ImageFromFormFileDto cnhImage);
    }
}
