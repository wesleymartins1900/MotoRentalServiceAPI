using MotoRentalService.Domain.ValueObjects;

namespace MotoRentalService.Domain.Entities
{
    public class DeliveryPerson
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Cnpj Cnpj { get; set; }
        public DateOnly BirthDate { get; set; }
        public Cnh Cnh { get; set; }
        public string CnhImageUrl { get; private set; }

        public void UpdateCnhImage(string? newCnhImageUrl)
        {
            CnhImageUrl = newCnhImageUrl ?? throw new ArgumentException("There is no image of the CNH to update.", nameof(newCnhImageUrl));
        }
    }
}
