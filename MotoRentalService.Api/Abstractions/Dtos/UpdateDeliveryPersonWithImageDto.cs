namespace MotoRentalService.Application.Dtos
{
    public class UpdateDeliveryPersonWithImageDto
    {
        public Guid Id {  get; set; }
        public IFormFile CnhImage { get; set; }
    }
}
