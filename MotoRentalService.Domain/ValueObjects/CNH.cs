using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Domain.ValueObjects
{
    public class Cnh
    {
        public string Number { get; private set; }
        public LicenseType Type { get; private set; }

        private Cnh() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cnh"/> class.
        /// </summary>
        /// <param name="number">The CNH number.</param>
        /// <param name="type">The CNH type as a string.</param>
        /// <exception cref="ArgumentException">Thrown when the CNH number or type is invalid.</exception>
        public Cnh(string number, string type)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("License number is incorrect.", nameof(number));

            Number = number;

            if (!Enum.TryParse(type, true, out LicenseType typeEnum))
                throw new ArgumentException("License type is incorrect.", nameof(type));

            Type = typeEnum;
        }

        /// <summary>
        /// Returns a string representation of the CNH in the format 'Number-Type'.
        /// </summary>
        /// <returns>A string representing the CNH.</returns>
        public override string ToString() => $"{Number}-{Type}";

        public override bool Equals(object obj)
        {
            if (obj is Cnh other)
                return Number == other.Number && Type == other.Type;
            
            return false;
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => HashCode.Combine(Number, Type);
    }
}
