using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Application.Dtos
{
    public class RentalMotoDto
    {
        public Guid DeliveryPersonId { get; set; }
        public Guid MotoId { get; set; }
        public DateTime EndDate { get; set; }
    }
}
