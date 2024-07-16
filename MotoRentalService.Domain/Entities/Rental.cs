using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Domain.Entities
{
    public class Rental
    {
        public Guid Id { get; set; }
        public Guid MotoId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public RentalPlanType PlanType { get; set; }
    }
}
