using MotoRentalService.Application.Dtos;
using MotoRentalService.Domain.Entities;
using MotoRentalService.CrossCutting.Messaging;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Application.Utils.CustomMessages;
using MotoRentalService.Domain.Shared;
using AutoMapper;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace MotoRentalService.Application.Services
{
    public class MotoService(IMotoRepository motoRepository,
                       IMessageProducer messageProducer,
                       IRentalService rentalService,
                       ILoggerManager logger,
                       IMapper mapper,
                       IMemoryCache memoryCache,
                       IValidator<RegisterMotoDto> registerValidator,
                       IValidator<UpdateMotoDto> updateValidator) : IMotoService
    {
        private readonly IMotoRepository _motoRepository = motoRepository;
        private readonly IMessageProducer _messageProducer = messageProducer;
        private readonly IRentalService _rentalService = rentalService;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _memoryCache = memoryCache; 
        private readonly IValidator<RegisterMotoDto> _registerValidator = registerValidator;
        private readonly IValidator<UpdateMotoDto> _updateValidator = updateValidator;
        private readonly MotoMessages _messages = new();

        /// <summary>
        /// Registers a new motorcycle asynchronously.
        /// </summary>
        /// <param name="motoDto">The data transfer object containing motorcycle details.</param>
        /// <returns>A <see cref="Result{Moto}"/> indicating the success or failure of the registration process.</returns>
        /// <remarks>
        /// This method checks if a motorcycle with the specified plate already exists. If not, it creates a new motorcycle record, publishes a validation message, and logs the event.
        /// </remarks>
        public async Task<Result<Moto>> RegisterMotoAsync(RegisterMotoDto motoDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(motoDto);
            if (!validationResult.IsValid)
            {
                var listErrosString = validationResult.Errors
                                                      .Select(error => error.ErrorMessage)
                                                      .Aggregate((current, next) => $"{current}\r\n{next}");

                _logger.LogWarn(listErrosString);
                return Result<Moto>.Failure(listErrosString);
            }

            var cacheKey = $"Moto_{motoDto.Plate}";
            if (_memoryCache.TryGetValue(cacheKey, out Moto cachedData))
            {
                _logger.LogWarn(_messages.CacheHit(cacheKey, $"Id: '{cachedData.Id}'"));
                return Result<Moto>.Failure(_messages.AlreadyExists(cachedData.Plate.ToString()));
            }

            var existingMoto = await _motoRepository.GetByPlateAsync(motoDto.Plate);
            if (existingMoto is not null)
            {
                _logger.LogWarn(_messages.AlreadyExists(existingMoto.Plate));
                return Result<Moto>.Failure(_messages.AlreadyExists(existingMoto.Plate));
            }

            var moto = _mapper.Map<Moto>(motoDto);
            moto.Id = Guid.NewGuid();

            await _messageProducer.PublishAsync("ValidateMotoRegistration", moto);

            _memoryCache.Set(cacheKey, moto, TimeSpan.FromMinutes(30));
            _logger.LogInfo(_messages.Published(nameof(motoDto.Plate), motoDto.Plate));

            return Result<Moto>.Success(moto);
        }

        /// <summary>
        /// Retrieves a paginated list of motorcycles based on the specified plate number.
        /// </summary>
        /// <param name="plate">Optional plate number to filter the motorcycles.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A <see cref="PagedResult{Moto}"/> containing the requested page of motorcycles.</returns>
        /// <remarks>
        /// This method delegates the retrieval of motorcycles to the repository, which returns a paginated result based on the provided parameters.
        /// </remarks>
        public async Task<PagedResult<Moto>> GetMotosAsync(string? plate, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var cacheKey = $"GetMotosAsync_{plate}_{pageNumber}_{pageSize}";

            if (_memoryCache.TryGetValue(cacheKey, out PagedResult<Moto> cachedMotos))
            {
                _logger.LogDebug(_messages.CacheHit(cacheKey, @$"Plate: '{plate}', 
                                                                 PageNumber: '{pageNumber}', 
                                                                 PageSize:'{pageSize}'"));
                return cachedMotos;
            }

            var result = await _motoRepository.GetMotosAsync(plate, pageNumber, pageSize);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), 
                SlidingExpiration = TimeSpan.FromSeconds(10)
            };

            _memoryCache.Set(cacheKey, result, cacheEntryOptions);
            return result;
        }

        /// <summary>
            /// Updates the details of an existing motorcycle.
            /// </summary>
            /// <param name="id">The unique identifier of the motorcycle to be updated.</param>
            /// <param name="motoDto">The data transfer object containing updated motorcycle details.</param>
            /// <returns>A <see cref="Result{Moto}"/> indicating the success or failure of the update operation.</returns>
            /// <remarks>
            /// This method retrieves the motorcycle by its ID. If found, it updates the motorcycle's details with the provided data, saves the changes to the repository, and logs the update. If the motorcycle is not found, it returns a failure result and logs the event.
            /// </remarks>
        public async Task<Result<Moto>> UpdateMotoAsync(Guid id, UpdateMotoDto motoDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(motoDto);
            if (!validationResult.IsValid)
            {
                var listErrosString = validationResult.Errors
                                                      .Select(error => error.ErrorMessage)
                                                      .Aggregate((current, next) => $"{current}\r\n{next}");

                _logger.LogWarn(listErrosString);
                return Result<Moto>.Failure(listErrosString);
            }

            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto is null)
            {
                _logger.LogWarn(_messages.NotFound(id));
                return Result<Moto>.Failure(_messages.NotFound(id));
            }

            moto.Plate = motoDto.Plate;

            await _motoRepository.UpdateAsync(moto);

            _memoryCache.Set($"Moto_{moto.Plate}", moto, TimeSpan.FromMinutes(30));
            _logger.LogInfo(_messages.Updated(nameof(motoDto.Plate), motoDto.Plate));

            return Result<Moto>.Success(moto);
        }

        /// <summary>
        /// Marks a motorcycle as deleted in the system.
        /// </summary>
        /// <param name="id">The unique identifier of the motorcycle to be deleted.</param>
        /// <returns>A <see cref="Result"/> indicating the success or failure of the delete operation.</returns>
        /// <remarks>
        /// This method checks if the motorcycle with the specified ID exists. If it does not exist, it returns a failure result and logs the event. If the motorcycle exists, it checks if there are any active rentals associated with it. If rentals are found, it returns a failure result indicating the presence of active rentals. If no rentals exist, it marks the motorcycle as deleted, updates the record in the repository, and logs the delete operation.
        /// </remarks>
        public async Task<Result> DeleteMotoAsync(Guid id)
        {
            var moto = await _motoRepository.GetByIdAsync(id);
            if (moto is null)
            {
                _logger.LogWarn(_messages.NotFound(id));
                return Result.Failure(_messages.NotFound(id));
            }

            var existsRent = await _rentalService.ExistsRentalsByMotoIdAsync(moto.Id);
            if (existsRent)
            {
                _logger.LogWarn(_messages.RentalAlreadyExists(id));
                return Result.Failure(_messages.RentalAlreadyExists(id));
            }

            moto.Deleted = true;
            await _motoRepository.UpdateAsync(moto);

            _memoryCache.Remove($"Moto_{moto.Plate}");

            _logger.LogInfo(_messages.Deleted(nameof(moto.Id), moto.Id.ToString()));

            return Result.Success();
        }
    }
}
