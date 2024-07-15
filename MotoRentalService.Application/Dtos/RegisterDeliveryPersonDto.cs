namespace MotoRentalService.Application.Dtos
{
    public class RegisterDeliveryPersonDto
    {
        public string Name { get; set; }
        public string Cnpj { get; set; }
        public DateOnly BirthDate { get; set; }
        public string CnhNumber { get; set; }
        public string CnhType { get; set; }
    }
}