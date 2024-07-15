using NSubstitute;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Application.Services;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.CrossCutting.Storage;
using MotoRentalService.Domain.ValueObjects;

namespace MotoRentalService.Tests.Unit.ServiceTests
{
    public class DeliveryPersonServiceTests
    {
        private readonly IDeliveryPersonRepository _deliveryPersonRepository;
        private readonly IStorageService _storageService;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly IValidator<RegisterDeliveryPersonDto> _validator;
        private readonly DeliveryPersonService _deliveryPersonService;

        public DeliveryPersonServiceTests()
        {
            _deliveryPersonRepository = Substitute.For<IDeliveryPersonRepository>();
            _storageService = Substitute.For<IStorageService>();
            _logger = Substitute.For<ILoggerManager>();
            _mapper = Substitute.For<IMapper>();
            _memoryCache = Substitute.For<IMemoryCache>();
            _validator = Substitute.For<IValidator<RegisterDeliveryPersonDto>>();

            _deliveryPersonService = new DeliveryPersonService(
                _deliveryPersonRepository,
                _storageService,
                _logger,
                _mapper,
                _memoryCache,
                _validator
            );
        }

        [Fact]
        public async Task RegisterDeliveryPersonAsync_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var deliveryPersonDto = new RegisterDeliveryPersonDto
            {
                Cnpj = "63.414.091/0001-21",
                Name = "Test",
                CnhNumber = "09876543210",
                CnhType = "C",
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };
            
            var validationResult = new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Cnpj", "CNPJ is invalid"),
                new FluentValidation.Results.ValidationFailure("CnhType", "CNH Type is invalid"),
            });
            _validator.ValidateAsync(deliveryPersonDto).Returns(Task.FromResult(validationResult));

            // Act
            var result = await _deliveryPersonService.RegisterDeliveryPersonAsync(deliveryPersonDto, cnhImage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("CNPJ is invalid", result.ErrorMessage);
            Assert.Contains("CNH Type is invalid", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterDeliveryPersonAsync_ShouldReturnFailure_WhenCnpjAlreadyExists()
        {
            // Arrange
            var deliveryPersonDto = new RegisterDeliveryPersonDto
            {
                Cnpj = "63.414.091/0001-21",
                Name = "Test",
                CnhNumber = "09876543210",
                CnhType = "B",
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };

            var existingDeliveryPerson = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Cnpj = new Cnpj("63414091000121"),
                Name = "Other Test",
                Cnh = new Cnh("09876543210", "B"),
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            _validator.ValidateAsync(deliveryPersonDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _mapper.Map<DeliveryPerson>(deliveryPersonDto).Returns(existingDeliveryPerson);
            _deliveryPersonRepository.GetByCnpjAsync(Arg.Any<string>()).Returns(Task.FromResult(existingDeliveryPerson));

            // Act
            var result = await _deliveryPersonService.RegisterDeliveryPersonAsync(deliveryPersonDto, cnhImage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("is already registered", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterDeliveryPersonAsync_ShouldReturnFailure_WhenCnhNumberAlreadyExists()
        {
            // Arrange
            var deliveryPersonDto = new RegisterDeliveryPersonDto
            {
                Cnpj = "63.414.091/0001-21",
                Name = "Test",
                CnhNumber = "09876543210",
                CnhType = "B",
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };

            var existingDeliveryPerson = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Cnpj = new Cnpj("63414091000121"),
                Name = "Other Test",
                Cnh = new Cnh("09876543210", "B"),
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            _validator.ValidateAsync(deliveryPersonDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _mapper.Map<DeliveryPerson>(deliveryPersonDto).Returns(existingDeliveryPerson);
            _deliveryPersonRepository.GetByCnpjAsync(Arg.Any<string>()).Returns(Task.FromResult((DeliveryPerson)null));
            _deliveryPersonRepository.GetByCnhNumberAsync(Arg.Any<string>()).Returns(Task.FromResult(existingDeliveryPerson));

            // Act
            var result = await _deliveryPersonService.RegisterDeliveryPersonAsync(deliveryPersonDto, cnhImage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("is already registered", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterDeliveryPersonAsync_ShouldReturnSuccess_WhenAllValidationsPass()
        {
            // Arrange
            var deliveryPersonDto = new RegisterDeliveryPersonDto
            {
                Cnpj = "63.414.091/0001-21",
                Name = "Test",
                CnhNumber = "09876543210",
                CnhType = "B",
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };

            var deliveryPerson = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Cnpj = new Cnpj("63.414.091/0001-21"),
                Name = "Other Test",
                Cnh = new Cnh("09876543210", "B"),
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            _validator.ValidateAsync(deliveryPersonDto).Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));
            _mapper.Map<DeliveryPerson>(deliveryPersonDto).Returns(deliveryPerson);
            _deliveryPersonRepository.GetByCnpjAsync(Arg.Any<string>()).Returns(Task.FromResult((DeliveryPerson)null));
            _deliveryPersonRepository.GetByCnhNumberAsync(Arg.Any<string>()).Returns(Task.FromResult((DeliveryPerson)null));
            _storageService.UploadAsync(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(Task.FromResult(StorageResult.Success("http://example.com/image.jpg")));

            // Act
            var result = await _deliveryPersonService.RegisterDeliveryPersonAsync(deliveryPersonDto, cnhImage);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(deliveryPerson.Id, result.Value.Id);
        }

        [Fact]
        public async Task UpdateDeliveryPersonAsync_ShouldReturnFailure_WhenDeliveryPersonNotFound()
        {
            // Arrange
            var deliveryPersonId = Guid.NewGuid();
            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };

            _deliveryPersonRepository.GetByIdAsync(deliveryPersonId).Returns(Task.FromResult<DeliveryPerson>(null));

            // Act
            var result = await _deliveryPersonService.UpdateDeliveryPersonAsync(deliveryPersonId, cnhImage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains($"DeliveryPerson with id '{deliveryPersonId}' not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateDeliveryPersonAsync_ShouldReturnSuccess_WhenUpdateSucceeds()
        {
            // Arrange
            var deliveryPersonId = Guid.NewGuid();
            var deliveryPerson = new DeliveryPerson
            {
                Id = deliveryPersonId,
                Cnpj = new Cnpj("63414091000121"),
                Name = "Test",
                Cnh = new Cnh("09876543210", "B"),
                BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-18)
            };

            var cnhImage = new ImageFromFormFileDto 
            { 
                FileName = "cnh.jpg", 
                Data = []
            };

            _deliveryPersonRepository.GetByIdAsync(deliveryPersonId).Returns(Task.FromResult(deliveryPerson));
            _storageService.UploadAsync(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(Task.FromResult(StorageResult.Success("http://example.com/image.jpg")));
            _deliveryPersonRepository.UpdateAsync(Arg.Any<DeliveryPerson>()).Returns(Task.CompletedTask);

            // Act
            var result = await _deliveryPersonService.UpdateDeliveryPersonAsync(deliveryPersonId, cnhImage);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
