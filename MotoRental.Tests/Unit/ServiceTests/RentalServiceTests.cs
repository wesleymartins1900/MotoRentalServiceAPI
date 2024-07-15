using MotoRentalService.Domain.Entities;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services;
using MotoRentalService.Domain.Factories;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using MotoRentalService.Domain.ValueObjects;
using MotoRentalService.Domain.Enums;
using MotoRentalService.Domain.Calculator;
using FluentAssertions;

namespace MotoRentalService.Tests.Unit.ServiceTests
{
    public class RentalServiceTests
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IMotoRepository _motoRepository;
        private readonly IDeliveryPersonRepository _deliveryPersonRepository;
        private readonly ILoggerManager _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IRentalCostCalculatorFactory _calculatorFactory;
        private readonly IValidator<RentalMotoDto> _rentalMotoDtoValidator;
        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            _rentalRepository = Substitute.For<IRentalRepository>();
            _motoRepository = Substitute.For<IMotoRepository>();
            _deliveryPersonRepository = Substitute.For<IDeliveryPersonRepository>();
            _logger = Substitute.For<ILoggerManager>();
            _memoryCache = Substitute.For<IMemoryCache>();
            _rentalMotoDtoValidator = Substitute.For<IValidator<RentalMotoDto>>();
            _calculatorFactory = Substitute.For<IRentalCostCalculatorFactory>();

            _rentalService = new RentalService(
                _rentalRepository,
                _motoRepository,
                _deliveryPersonRepository,
                _logger,
                _memoryCache,
                _rentalMotoDtoValidator,
                _calculatorFactory
                );
        }

        [Fact]
        public async Task RegisterRentalMotoAsync_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var rentalDto = new RentalMotoDto 
            { 
                DeliveryPersonId = Guid.NewGuid(), 
                MotoId = Guid.NewGuid() 
            };
            
            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("DeliveryPersonId", "DeliveryPersonId is invalid")
            });
            _rentalMotoDtoValidator.ValidateAsync(rentalDto).Returns(Task.FromResult(validationResult));

            // Act
            var result = await _rentalService.RegisterRentalMotoAsync(rentalDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("DeliveryPersonId is invalid", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterRentalMotoAsync_ShouldReturnFailure_WhenDeliveryPersonNotFound()
        {
            // Arrange
            var rentalDto = new RentalMotoDto 
            { 
                DeliveryPersonId = Guid.NewGuid(), 
                MotoId = Guid.NewGuid(),
                EndDate = DateTime.Now.AddDays(8),
            };

            _rentalMotoDtoValidator.ValidateAsync(rentalDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _deliveryPersonRepository.GetByIdAsync(rentalDto.DeliveryPersonId).Returns(Task.FromResult((DeliveryPerson)null));

            // Act
            var result = await _rentalService.RegisterRentalMotoAsync(rentalDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Delivery person with ID '{rentalDto.DeliveryPersonId}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterRentalMotoAsync_ShouldReturnFailure_WhenDeliveryPersonNoLicenseTypeA()
        {
            // Arrange
            var rentalDto = new RentalMotoDto 
            { 
                DeliveryPersonId = Guid.NewGuid(), 
                MotoId = Guid.NewGuid() ,
                EndDate = DateTime.Now.AddDays(8)
            };

            var deliveryPerson = new DeliveryPerson 
            {   
                Id = rentalDto.DeliveryPersonId, 
                Cnh = new Cnh("09876543210", "B"),
            };

            _rentalMotoDtoValidator.ValidateAsync(rentalDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _deliveryPersonRepository.GetByIdAsync(rentalDto.DeliveryPersonId).Returns(Task.FromResult(deliveryPerson));

            // Act
            var result = await _rentalService.RegisterRentalMotoAsync(rentalDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Delivery person with ID '{rentalDto.DeliveryPersonId}' does not have a type A license.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterRentalMotoAsync_ShouldReturnFailure_WhenMotoNotFound()
        {
            // Arrange
            var rentalDto = new RentalMotoDto 
            { 
                DeliveryPersonId = Guid.NewGuid(), 
                MotoId = Guid.NewGuid(),
                EndDate = DateTime.Now.AddDays(8)
            };

            var deliveryPerson = new DeliveryPerson 
            { 
                Id = rentalDto.DeliveryPersonId, 
                Cnh = new Cnh("09876543210", "A"),
            };
            
            _rentalMotoDtoValidator.ValidateAsync(rentalDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _deliveryPersonRepository.GetByIdAsync(rentalDto.DeliveryPersonId).Returns(Task.FromResult(deliveryPerson));
            _motoRepository.GetByIdAsync(rentalDto.MotoId).Returns(Task.FromResult((Moto)null));

            // Act
            var result = await _rentalService.RegisterRentalMotoAsync(rentalDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Moto with ID '{rentalDto.MotoId}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterRentalMotoAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var rentalDto = new RentalMotoDto 
            { 
                DeliveryPersonId = Guid.NewGuid(), 
                MotoId = Guid.NewGuid(), 
                EndDate = DateTime.Now.AddDays(8), 
            };

            var deliveryPerson = new DeliveryPerson 
            { 
                Id = rentalDto.DeliveryPersonId, 
                Cnh = new Cnh("09876543210", "A"),
            };

            var moto = new Moto { Id = rentalDto.MotoId };

            _rentalMotoDtoValidator.ValidateAsync(rentalDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _deliveryPersonRepository.GetByIdAsync(rentalDto.DeliveryPersonId).Returns(Task.FromResult(deliveryPerson));
            _motoRepository.GetByIdAsync(rentalDto.MotoId).Returns(Task.FromResult(moto));

            // Act
            var result = await _rentalService.RegisterRentalMotoAsync(rentalDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(rentalDto.MotoId, result.Value.MotoId);
        }

        [Fact]
        public async Task ExistsRentalsByMotoIdAsync_ShouldReturnTrue_WhenRentalsExist()
        {
            // Arrange
            var motoId = Guid.NewGuid();
            
            _rentalRepository.ExistsRentalsByMotoIdAsync(motoId).Returns(Task.FromResult(true));

            // Act
            var result = await _rentalService.ExistsRentalsByMotoIdAsync(motoId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsRentalsByMotoIdAsync_ShouldReturnFalse_WhenNoRentalsExist()
        {
            // Arrange
            var motoId = Guid.NewGuid();

            _rentalRepository.ExistsRentalsByMotoIdAsync(motoId).Returns(Task.FromResult(false));

            // Act
            var result = await _rentalService.ExistsRentalsByMotoIdAsync(motoId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RentalValueCalculateAsync_ShouldReturnFailure_WhenRentalNotFound()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var endDate = DateTime.Now;

            _rentalRepository.GetByIdAsync(rentalId).Returns(Task.FromResult((Rental)null));

            // Act
            var result = await _rentalService.RentalValueCalculateAsync(rentalId, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Rental with id '{rentalId}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task RentalValueCalculateAsync_ShouldReturnFailure_WhenEndDateBeforeStartDate()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var endDate = DateTime.Now;

            var rental = new Rental 
            { 
                Id = rentalId, 
                StartDate = DateTime.Now.Date.AddDays(2) 
            };

            _rentalRepository.GetByIdAsync(rentalId).Returns(Task.FromResult(rental));

            // Act
            var result = await _rentalService.RentalValueCalculateAsync(rentalId, endDate);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"End date '{endDate}' is before the start date '{rental.StartDate}'", result.ErrorMessage);
        }

        [Theory]
        [InlineData(RentalPlanType.SevenDays, 186)]
        [InlineData(RentalPlanType.FifteenDays, 403.2)]
        [InlineData(RentalPlanType.ThirtyDays, 638)]
        [InlineData(RentalPlanType.FortyFiveDays, 880)]
        [InlineData(RentalPlanType.FiftyDays, 882)]
        public async Task RentalValueCalculateAsync_ShouldReturnSuccess_WhenEarlyReturnCalculationSucceeds(
            RentalPlanType planType, decimal expectedCost)
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.Now.Date.AddDays(1); 
            var endDate = startDate.AddDays((int)planType); 
            var expectedEndDate = endDate.AddDays(-1); // expected date before end date (EarlyReturn)

            var rental = new Rental
            {
                Id = rentalId,
                StartDate = startDate,
                EndDate = endDate,
                PlanType = planType
            };

            var calculator = new EarlyReturnRentalCostCalculator();
            _calculatorFactory.ChooseCalculatorBasedOnReturnDate(Arg.Any<Rental>(), Arg.Any<DateTime>())
                .Returns(calculator);

            _rentalRepository.GetByIdAsync(rentalId).Returns(Task.FromResult(rental));

            // Act
            var result = await _rentalService.RentalValueCalculateAsync(rentalId, expectedEndDate);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedCost);
        }

        [Theory]
        [InlineData(RentalPlanType.SevenDays, 290)]
        [InlineData(RentalPlanType.FifteenDays, 498)]
        [InlineData(RentalPlanType.ThirtyDays, 732)]
        [InlineData(RentalPlanType.FortyFiveDays, 970)]
        [InlineData(RentalPlanType.FiftyDays, 968)]
        public async Task RentalValueCalculateAsync_ShouldReturnSuccess_WhenLateReturnCalculationSucceeds(
            RentalPlanType planType, decimal expectedCost)
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.Now.Date.AddDays(1); 
            var endDate = startDate.AddDays((int)planType); 
            var expectedEndDate = endDate.AddDays(1); // Expected date after end date (LateReturn)

            var rental = new Rental
            {
                Id = rentalId,
                StartDate = startDate,
                EndDate = endDate,
                PlanType = planType
            };

            var calculator = new LateReturnRentalCostCalculator();
            _calculatorFactory.ChooseCalculatorBasedOnReturnDate(Arg.Any<Rental>(), Arg.Any<DateTime>())
                .Returns(calculator);

            _rentalRepository.GetByIdAsync(rentalId).Returns(Task.FromResult(rental));

            // Act
            var result = await _rentalService.RentalValueCalculateAsync(rentalId, expectedEndDate);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedCost);
        }

        [Theory]
        [InlineData(RentalPlanType.SevenDays, 210)]
        [InlineData(RentalPlanType.FifteenDays, 420)]
        [InlineData(RentalPlanType.ThirtyDays, 660)]
        [InlineData(RentalPlanType.FortyFiveDays, 900)]
        [InlineData(RentalPlanType.FiftyDays, 900)]
        public async Task RentalValueCalculateAsync_ShouldReturnSuccess_WhenOnTimeReturnCalculationSucceeds(
            RentalPlanType planType, decimal expectedCost)
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.Now.Date.AddDays(1); 
            var endDate = startDate.AddDays((int)planType); 
            var expectedEndDate = endDate; // Expected date equals end date (OnTime)

            var rental = new Rental
            {
                Id = rentalId,
                StartDate = startDate,
                EndDate = endDate, 
                PlanType = planType
            };

            var calculator = new OnTimeReturnRentalCostCalculator();
            _calculatorFactory.ChooseCalculatorBasedOnReturnDate(Arg.Any<Rental>(), Arg.Any<DateTime>())
                .Returns(calculator);

            _rentalRepository.GetByIdAsync(rentalId).Returns(Task.FromResult(rental));

            // Act
            var result = await _rentalService.RentalValueCalculateAsync(rentalId, expectedEndDate);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedCost);
        }

    }
}
