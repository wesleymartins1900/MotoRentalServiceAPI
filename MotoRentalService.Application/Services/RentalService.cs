using MotoRentalService.Domain.Entities;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.Application.Dtos;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Application.Utils.CustomMessages;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using MotoRentalService.Domain.Factories;
using MotoRentalService.Domain.Enums;

namespace MotoRentalService.Application.Services
{
    public class RentalService(IRentalRepository rentalRepository,
                         IMotoRepository motoRepository,
                         IDeliveryPersonRepository deliveryPersonRepository,
                         ILoggerManager logger,
                         IMemoryCache memoryCache,
                         IValidator<RentalMotoDto> rentalMotoDtoValidator,
                         IRentalCostCalculatorFactory calculatorFactory) : IRentalService
    {
        private readonly IRentalRepository _rentalRepository = rentalRepository;
        private readonly IMotoRepository _motoRepository = motoRepository;
        private readonly IDeliveryPersonRepository _deliveryPersonRepository = deliveryPersonRepository;
        private readonly ILoggerManager _logger = logger;
        private readonly IValidator<RentalMotoDto> _rentalMotoDtoValidator = rentalMotoDtoValidator;
        private readonly IMemoryCache _memoryCache = memoryCache; 
        private readonly RentalMessages _messages = new();
        private readonly IRentalCostCalculatorFactory _calculatorFactory = calculatorFactory;

        /// <summary>
        /// Calculates the total rental cost based on the rental period and the actual return date.
        /// </summary>
        /// <param name="rentalId">The unique identifier of the rental.</param>
        /// <param name="returnDate">The date when the rental was returned.</param>
        /// <returns>A <see cref="Result{decimal}"/> containing the total rental cost or an error message if the rental is not found.</returns>
        /// <remarks>
        /// Computes the total rental cost by comparing the actual return date with the expected end date of the rental.
        /// </remarks>
        private async Task<Result<decimal>> CalculateRentalCostAsync(Guid rentalId, DateTime returnDate)
        {
            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental is null)
                return Result<decimal>.Failure(_messages.NotFound(rentalId));

            var calculator = _calculatorFactory.ChooseCalculatorBasedOnReturnDate(rental, returnDate);
            var totalCost = calculator.Calculate(rental, returnDate);

            return Result<decimal>.Success(totalCost);
        }

        /// <summary>
        /// Calculates the number of days between the given start date and end date.
        /// Checks if the calculated number of days matches a defined value in the <see cref="RentalPlanType"/> enum.
        /// </summary>
        /// <param name="startDate">The start date of the rental period.</param>
        /// <param name="endDate">The end date of the rental period.</param>
        /// <returns>
        /// A <see cref="Result{RentalPlanType}"/> indicating success with the corresponding <see cref="RentalPlanType"/> 
        /// if the number of days is valid, or failure with an error message if the number of days is invalid.
        /// </returns>
        private Result<RentalPlanType> CheckRentalPlanType(DateTime startDate, DateTime endDate)
        {
            var planTypeDays = (endDate - startDate).Days;

            if (!Enum.IsDefined(typeof(RentalPlanType), planTypeDays))
                return Result<RentalPlanType>.Failure("Invalid rental plan type.");

            var planType = (RentalPlanType)planTypeDays;
            return Result<RentalPlanType>.Success(planType);
        }

        /// <summary>
        /// Creates a new rental record for a motorcycle.
        /// </summary>
        /// <param name="rentalMotoDto">The data transfer object containing details for the rental.</param>
        /// <returns>A <see cref="Result{Rental}"/> containing the created rental or an error message if the creation fails.</returns>
        /// <remarks>
        /// Verifies the delivery person and motorcycle exist before creating a new rental record.
        /// </remarks>
        public async Task<Result<Rental>> RegisterRentalMotoAsync(RentalMotoDto rentalMotoDto)
        {
            var validationResult = await _rentalMotoDtoValidator.ValidateAsync(rentalMotoDto);
            if (!validationResult.IsValid)
            {
                var listErrosString = validationResult.Errors
                                                      .Select(error => error.ErrorMessage)
                                                      .Aggregate((current, next) => $"{current}\r\n{next}");

                _logger.LogWarn(listErrosString);
                return Result<Rental>.Failure(listErrosString);
            }

            var startDate = DateTime.Now.Date.AddDays(1);
            if (!CheckRentalPlanType(startDate, rentalMotoDto.EndDate).IsSuccess)
            {
                _logger.LogWarn(_messages.InvalidRentalPlanType(startDate, rentalMotoDto.EndDate));
                return Result<Rental>.Failure(_messages.InvalidRentalPlanType(startDate, rentalMotoDto.EndDate));
            }

            var cacheKey = $"Rental_{rentalMotoDto.DeliveryPersonId}";
            if (_memoryCache.TryGetValue(cacheKey, out Rental cachedData))
            {
                _logger.LogWarn(_messages.CacheHit(cacheKey, $"Id: '{cachedData.Id}'"));
                return Result<Rental>.Failure(_messages.AlreadyExists(cachedData.Id.ToString()));
            }

            var deliveryPerson = await _deliveryPersonRepository.GetByIdAsync(rentalMotoDto.DeliveryPersonId);
            if (deliveryPerson is null)
            {
                _logger.LogWarn(_messages.DeliveryPersonNotFound(rentalMotoDto.DeliveryPersonId));
                return Result<Rental>.Failure(_messages.DeliveryPersonNotFound(rentalMotoDto.DeliveryPersonId));
            }

            if (!deliveryPerson.Cnh.Type.ToString().Contains('A'))
            {
                _logger.LogWarn(_messages.DeliveryPersonNoLicenseTypeA(rentalMotoDto.DeliveryPersonId));
                return Result<Rental>.Failure(_messages.DeliveryPersonNoLicenseTypeA(rentalMotoDto.DeliveryPersonId));
            }

            var moto = await _motoRepository.GetByIdAsync(rentalMotoDto.MotoId);
            if (moto is null)
            {
                _logger.LogWarn(_messages.MotoNotFound(rentalMotoDto.MotoId));
                return Result<Rental>.Failure(_messages.MotoNotFound(rentalMotoDto.MotoId));
            }

            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                DeliveryPersonId = deliveryPerson.Id,
                MotoId = moto.Id,
                StartDate = startDate,
                EndDate = rentalMotoDto.EndDate
            };

            await _rentalRepository.AddAsync(rental);

            _memoryCache.Set(cacheKey, rentalMotoDto, TimeSpan.FromMinutes(30));
            _logger.LogInfo(_messages.RentalCreatedSuccessfully(rental.Id, deliveryPerson.Name));

            return Result<Rental>.Success(rental);
        }

        /// <summary>
        /// Checks if there are any rentals associated with a specific motorcycle.
        /// </summary>
        /// <param name="motoId">The unique identifier of the motorcycle.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if rentals exist; otherwise, false.</returns>
        /// <remarks>
        /// Queries the repository to check if any rentals exist for the given motorcycle ID.
        /// </remarks>
        public async Task<bool> ExistsRentalsByMotoIdAsync(Guid motoId) => await _rentalRepository.ExistsRentalsByMotoIdAsync(motoId);

        /// <summary>
        /// Calculates the rental value based on the rental ID and the end date.
        /// </summary>
        /// <param name="id">The unique identifier of the rental.</param>
        /// <param name="endDate">The date when the rental ended.</param>
        /// <returns>A <see cref="Result{decimal}"/> containing the calculated rental value or an error message if the calculation fails.</returns>
        /// <remarks>
        /// Calculates the rental value by determining the total rental days and applying the appropriate cost calculator based on the end date.
        /// </remarks>
        public async Task<Result<decimal>> RentalValueCalculateAsync(Guid id, DateTime expectedEndDate)
        {
            var rental = await _rentalRepository.GetByIdAsync(id);
            if (rental is null)
            {
                _logger.LogWarn(_messages.NotFound(id));
                return Result<decimal>.Failure(_messages.NotFound(id));
            }

            var totalDays = (expectedEndDate - rental.StartDate).Days;
            if (totalDays < 0)
            {
                _logger.LogWarn(_messages.EndDateBeforeStartDate(expectedEndDate, rental.StartDate));
                return Result<decimal>.Failure(_messages.EndDateBeforeStartDate(expectedEndDate, rental.StartDate));
            }

            var totalCost = await CalculateRentalCostAsync(id, expectedEndDate);
            if (!totalCost.IsSuccess)
            {
                _logger.LogError(_messages.RentalCalculationError(id));
                return Result<decimal>.Failure(_messages.RentalCalculationError(id));
            }

            _logger.LogInfo(_messages.RentalValueCalculatedSuccessfully(id, totalCost.Value));
            return Result<decimal>.Success(totalCost.Value);
        }
    }
}
