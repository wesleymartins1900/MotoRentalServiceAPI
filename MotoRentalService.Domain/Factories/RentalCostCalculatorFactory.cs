using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Domain.Calculator;
using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Domain.Factories
{
    /// <summary>
    /// Factory class for choosing the appropriate rental cost calculator based on the return date of the rental.
    /// </summary>
    public class RentalCostCalculatorFactory : IRentalCostCalculatorFactory
    {
        private readonly EarlyReturnRentalCostCalculator _earlyReturnCalculator;
        private readonly LateReturnRentalCostCalculator _lateReturnCalculator;
        private readonly OnTimeReturnRentalCostCalculator _onTimeReturnCalculator;

        public RentalCostCalculatorFactory(
                    EarlyReturnRentalCostCalculator earlyReturnCalculator,
                    LateReturnRentalCostCalculator lateReturnCalculator,
                    OnTimeReturnRentalCostCalculator onTimeReturnCalculator)
        {
            _earlyReturnCalculator = earlyReturnCalculator;
            _lateReturnCalculator = lateReturnCalculator;
            _onTimeReturnCalculator = onTimeReturnCalculator;
        }

        /// <summary>
        /// Chooses the appropriate rental cost calculator based on the comparison of the return date with the expected end date.
        /// </summary>
        /// <param name="rental">The rental entity containing the expected end date.</param>
        /// <param name="returnDate">The actual return date of the rental.</param>
        /// <returns>An instance of <see cref="IRentalCostCalculator"/> suited for the given return date.</returns>
        public IRentalCostCalculator ChooseCalculatorBasedOnReturnDate(Rental rental, DateTime returnDate)
        {
            if (returnDate < rental.EndDate)
            {
                return _earlyReturnCalculator;
            }
            else if (returnDate > rental.EndDate)
            {
                return _lateReturnCalculator;
            }
            else
            {
                return _onTimeReturnCalculator;
            }
        }
    }
}
