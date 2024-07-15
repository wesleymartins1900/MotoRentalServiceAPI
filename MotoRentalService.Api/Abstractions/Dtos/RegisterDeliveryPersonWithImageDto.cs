using MotoRentalService.Application.Dtos;

namespace MotoRentalService.Api.Abstractions.Dtos
{
    public class RegisterDeliveryPersonWithImageDto : RegisterDeliveryPersonDto
    {
        public IFormFile CnhImage { get; set; }
    }
}
