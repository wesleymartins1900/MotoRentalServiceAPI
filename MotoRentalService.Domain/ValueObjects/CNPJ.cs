using System.Text.RegularExpressions;

namespace MotoRentalService.Domain.ValueObjects
{
    public class Cnpj
    {
        public string Value { get; private set; }

        private Cnpj() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cnpj"/> class.
        /// </summary>
        /// <param name="value">The CNPJ value.</param>
        /// <exception cref="ArgumentException">Thrown when the CNPJ value is invalid.</exception>
        public Cnpj(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("CNPJ value cannot be null or whitespace.", nameof(value));

            var cleanedValue = Regex.Replace(value, "[^0-9]", "");

            if (!IsValidCnpj(cleanedValue))
                throw new ArgumentException("Invalid CNPJ format.", nameof(cleanedValue));

            Value = cleanedValue;
        }

        private bool IsValidCnpj(string cnpj)
        {
            if (cnpj.Length != 14 || !cnpj.All(char.IsDigit))
                return false;

            // Padrões de pesos para os dígitos verificadores
            int[] firstWeights = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] secondWeights = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            return VerifyDigit(cnpj, firstWeights, 12) && VerifyDigit(cnpj, secondWeights, 13);
        }

        private bool VerifyDigit(string cnpj, int[] weights, int position)
        {
            int sum = cnpj.Take(position)
                           .Select((c, i) => (c - '0') * weights[i])
                           .Sum();

            int expectedDigit = (sum % 11 < 2) ? 0 : 11 - (sum % 11);

            return cnpj[position] == (char)(expectedDigit + '0');
        }


        /// <summary>
        /// Checks if the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            // Check if the provided object is null or if it is not of type Cnpj
            if (obj is not Cnpj other)
                return false;

            // Check if the current instance and the other instance have the same Value
            return Value == other.Value;
        }

        /// <summary>
        /// Gets the hash code for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => HashCode.Combine(Value);

        /// <summary>
        /// Returns a string representation of the CNPJ.
        /// </summary>
        /// <returns>A string representing the CNPJ.</returns>
        public override string ToString() => Value;
    }
}
