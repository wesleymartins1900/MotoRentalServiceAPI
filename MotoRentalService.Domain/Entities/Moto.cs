namespace MotoRentalService.Domain.Entities
{
    public class Moto
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string Model { get; set; }
        public string Plate { get; set; }
        public bool Deleted { get; set; }
    }
}
