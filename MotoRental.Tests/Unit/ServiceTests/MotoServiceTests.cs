using MotoRentalService.Application.Dtos;
using MotoRentalService.Domain.Entities;
using MotoRentalService.CrossCutting.Messaging;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.Domain.Shared;
using AutoMapper;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using MotoRentalService.Application.Services;

namespace MotoRentalService.Tests.Unit.ServiceTests
{
    public class MotoServiceTests
    {
        private readonly IMotoRepository _motoRepository;
        private readonly IMessageProducer _messageProducer;
        private readonly IRentalService _rentalService;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly IValidator<RegisterMotoDto> _registerValidator;
        private readonly IValidator<UpdateMotoDto> _updateValidator;
        private readonly MotoService _motoService;

        public MotoServiceTests()
        {
            _motoRepository = Substitute.For<IMotoRepository>();
            _messageProducer = Substitute.For<IMessageProducer>();
            _rentalService = Substitute.For<IRentalService>();
            _logger = Substitute.For<ILoggerManager>();
            _mapper = Substitute.For<IMapper>();
            _memoryCache = Substitute.For<IMemoryCache>();
            _registerValidator = Substitute.For<IValidator<RegisterMotoDto>>();
            _updateValidator = Substitute.For<IValidator<UpdateMotoDto>>();

            _motoService = new MotoService(
                _motoRepository,
                _messageProducer,
                _rentalService,
                _logger,
                _mapper,
                _memoryCache,
                _registerValidator,
                _updateValidator
            );
        }

        [Fact]
        public async Task RegisterMotoAsync_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var motoDto = new RegisterMotoDto { Plate = "ABC-1234" };

            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Plate", "Plate is invalid")
            });
            _registerValidator.ValidateAsync(motoDto).Returns(Task.FromResult(validationResult));
            _messageProducer.PublishAsync("ValidateMotoRegistration", Arg.Any<Moto>()).Returns(Task.CompletedTask);

            // Act
            var result = await _motoService.RegisterMotoAsync(motoDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Plate is invalid", result.ErrorMessage);
         
            await _messageProducer.DidNotReceive().PublishAsync("ValidateMotoRegistration", Arg.Any<Moto>());
        }

        [Fact]
        public async Task RegisterMotoAsync_ShouldReturnFailure_WhenMotoAlreadyExists()
        {
            // Arrange
            var motoDto = new RegisterMotoDto { Plate = "ABC-1234" };

            _registerValidator.ValidateAsync(motoDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _motoRepository.GetByPlateAsync(motoDto.Plate).Returns(Task.FromResult(new Moto { Plate = motoDto.Plate}));
            _messageProducer.PublishAsync("ValidateMotoRegistration", Arg.Any<Moto>()).Returns(Task.CompletedTask);

            // Act
            var result = await _motoService.RegisterMotoAsync(motoDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"A motorcycle with the plate '{motoDto.Plate}' is already registered.", result.ErrorMessage);
         
            await _messageProducer.DidNotReceive().PublishAsync("ValidateMotoRegistration", Arg.Any<Moto>());
        }

        [Fact]
        public async Task RegisterMotoAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var motoDto = new RegisterMotoDto { Plate = "ABC-1234" };

            var moto = new Moto
            {
                Id = Guid.NewGuid(),
                Plate = motoDto.Plate
            };

            _registerValidator.ValidateAsync(motoDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _motoRepository.GetByPlateAsync(motoDto.Plate).Returns(Task.FromResult((Moto)null));
            _mapper.Map<Moto>(motoDto).Returns(moto);
            _messageProducer.PublishAsync("ValidateMotoRegistration", moto).Returns(Task.CompletedTask);

            // Act
            var result = await _motoService.RegisterMotoAsync(motoDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(moto.Id, result.Value.Id);
        }


        [Fact]
        public async Task GetMotosAsync_ShouldReturnCachedResult_WhenCacheHit()
        {
            // Arrange
            var plate = "ABC-1234";
            var pageNumber = 1;
            var pageSize = 10;
            var cacheKey = $"GetMotosAsync_{plate}_{pageNumber}_{pageSize}";
            var cachedResult = new PagedResult<Moto>();

            _memoryCache.TryGetValue(cacheKey, out Arg.Any<PagedResult<Moto>>())
                        .Returns(x =>
                        {
                            x[1] = cachedResult;
                            return true;
                        });

            // Act
            var result = await _motoService.GetMotosAsync(plate, pageNumber, pageSize);

            // Assert
            Assert.Equal(cachedResult, result);
        }

        [Fact]
        public async Task GetMotosAsync_ShouldReturnPaginatedResult_WhenCacheMiss()
        {
            // Arrange
            var plate = "ABC-1234";
            var pageNumber = 1;
            var pageSize = 10;
            var cacheKey = $"GetMotosAsync_{plate}_{pageNumber}_{pageSize}";
            var pagedResult = new PagedResult<Moto>();

            _memoryCache.TryGetValue(cacheKey, out Arg.Any<PagedResult<Moto>>()).Returns(false);
            _motoRepository.GetMotosAsync(plate, pageNumber, pageSize).Returns(Task.FromResult(pagedResult));

            // Act
            var result = await _motoService.GetMotosAsync(plate, pageNumber, pageSize);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task UpdateMotoAsync_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var motoDto = new UpdateMotoDto { Plate = "ABC-1234" };

            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Plate", "Plate is invalid")
            });
            _updateValidator.ValidateAsync(motoDto).Returns(Task.FromResult(validationResult));

            // Act
            var result = await _motoService.UpdateMotoAsync(id, motoDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Plate is invalid", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateMotoAsync_ShouldReturnFailure_WhenMotoNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var motoDto = new UpdateMotoDto { Plate = "ABC-1234" };

            _updateValidator.ValidateAsync(motoDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _motoRepository.GetByIdAsync(id).Returns(Task.FromResult((Moto)null));

            // Act
            var result = await _motoService.UpdateMotoAsync(id, motoDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Moto with Id '{id}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateMotoAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var id = Guid.NewGuid();
            var motoDto = new UpdateMotoDto { Plate = "ABC-1234" };

            var moto = new Moto 
            { 
                Id = id, 
                Plate = motoDto.Plate 
            };

            _updateValidator.ValidateAsync(motoDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _motoRepository.GetByIdAsync(id).Returns(Task.FromResult(moto));

            // Act
            var result = await _motoService.UpdateMotoAsync(id, motoDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(moto.Id, result.Value.Id);
        }

        [Fact]
        public async Task DeleteMotoAsync_ShouldReturnFailure_WhenMotoNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _motoRepository.GetByIdAsync(id).Returns(Task.FromResult((Moto)null));

            // Act
            var result = await _motoService.DeleteMotoAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Moto with Id '{id}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMotoAsync_ShouldReturnFailure_WhenActiveRentalsExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var moto = new Moto 
            { 
                Id = id, 
                Plate = "ABC-1234" 
            };

            _motoRepository.GetByIdAsync(id).Returns(Task.FromResult(moto));
            _rentalService.ExistsRentalsByMotoIdAsync(moto.Id).Returns(Task.FromResult(true));

            // Act
            var result = await _motoService.DeleteMotoAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"Moto with Id '{id}' has an existing rental.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMotoAsync_ShouldReturnSuccess_WhenMotoDeletedSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var moto = new Moto 
            { 
                Id = id, 
                Plate = "ABC-1234" 
            };

            _motoRepository.GetByIdAsync(id).Returns(Task.FromResult(moto));
            _rentalService.ExistsRentalsByMotoIdAsync(moto.Id).Returns(Task.FromResult(false));

            // Act
            var result = await _motoService.DeleteMotoAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
