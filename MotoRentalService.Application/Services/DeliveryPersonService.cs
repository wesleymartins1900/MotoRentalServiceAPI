using AutoMapper;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Application.Utils.CustomMessages;
using MotoRentalService.CrossCutting.Storage;
using MotoRentalService.CrossCutting.Logging;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace MotoRentalService.Application.Services
{
    public class DeliveryPersonService(IDeliveryPersonRepository deliveryPersonRepository,
                                 IStorageService storageService,
                                 ILoggerManager logger,
                                 IMapper mapper,
                                 IMemoryCache memoryCache,
                                 IValidator<RegisterDeliveryPersonDto> validator) : IDeliveryPersonService
    {
        private readonly IDeliveryPersonRepository _deliveryPersonRepository = deliveryPersonRepository;
        private readonly IStorageService _storageService = storageService;
        private readonly ILoggerManager _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IValidator<RegisterDeliveryPersonDto> _validator = validator;
        private readonly DeliveryPersonMessages _messages = new();

        /// <summary>
        /// Asynchronously uploads a CNH (National Driver's License) image to the storage service.
        /// </summary>
        /// <param name="id">The identifier associated with the CNH image upload.</param>
        /// <param name="cnhImage">An instance of <see cref="ImageFromFormFileDto"/> containing the image file name and data.</param>
        /// <returns>
        /// A <see cref="StorageResult"/> indicating the success or failure of the upload operation. 
        /// If successful, the result includes the URL of the uploaded image. 
        /// If failed, the result contains the error message.
        /// </returns>
        /// <remarks>
        /// This method uses the provided storage service to upload the image and logs any errors encountered during the process.
        /// </remarks>
        private async Task<StorageResult> UploadCnhImageAsync(Guid id, ImageFromFormFileDto cnhImage)
        {
            if (cnhImage is null)
            {
                _logger.LogWarn(_messages.UploadImageNullError(id));
                return StorageResult.Failure(_messages.UploadImageNullError(id));
            }

            var uploadResult = await _storageService.UploadAsync(cnhImage.FileName, cnhImage.Data);
            if (!uploadResult.IsSuccess)
            {
                _logger.LogWarn(_messages.UploadImageGenericError(id));
                return StorageResult.Failure(_messages.UploadImageGenericError(id));
            }

            return StorageResult.Success(uploadResult.Url);
        }

        /// <summary>
        /// Asynchronously register a new delivery person record and uploads their CNH (National Driver's License) image.
        /// </summary>
        /// <param name="deliveryPersonDto">An instance of <see cref="RegisterDeliveryPersonDto"/> containing the delivery person's details.</param>
        /// <param name="cnhImage">An instance of <see cref="ImageFromFormFileDto"/> containing the CNH image file name and data.</param>
        /// <returns>
        /// A <see cref="Result{DeliveryPerson}"/> indicating the success or failure of the creation operation. 
        /// If successful, the result includes the created <see cref="DeliveryPerson"/> object. 
        /// If failed, the result contains an error message.
        /// </returns>
        /// <remarks>
        /// Saves the new delivery person record to the repository and logs the registration success.
        /// </remarks>
        public async Task<Result<DeliveryPerson>> RegisterDeliveryPersonAsync(RegisterDeliveryPersonDto deliveryPersonDto, ImageFromFormFileDto cnhImage)
        {
            var validationResult = await _validator.ValidateAsync(deliveryPersonDto);
            if (!validationResult.IsValid)
            {
                var listErrosString = validationResult.Errors
                                                      .Select(error => error.ErrorMessage)
                                                      .Aggregate((current, next) => $"{current}\r\n{next}");

                _logger.LogWarn(listErrosString);
                return Result<DeliveryPerson>.Failure(listErrosString);
            }

            var deliveryPerson = _mapper.Map<DeliveryPerson>(deliveryPersonDto);
            deliveryPerson.Id = Guid.NewGuid();

            var cacheKey = $"DeliveryPerson_{deliveryPerson.Cnpj}";
            if (_memoryCache.TryGetValue(cacheKey, out DeliveryPerson cachedData))
            {
                _logger.LogWarn(_messages.CacheHit(cacheKey, $"Id: '{cachedData.Id}'"));
                return Result<DeliveryPerson>.Failure(_messages.AlreadyExists(cachedData.Cnpj.ToString()));
            }

            var otherCnpjDeliveryPerson = await _deliveryPersonRepository.GetByCnpjAsync(deliveryPerson.Cnpj.ToString());
            if (otherCnpjDeliveryPerson is not null)
            {
                _logger.LogWarn(_messages.AlreadyExists(otherCnpjDeliveryPerson.Cnpj.ToString()));
                return Result<DeliveryPerson>.Failure(_messages.AlreadyExists(otherCnpjDeliveryPerson.Cnpj.ToString()));
            }

            var otherCnhDeliveryPerson = await _deliveryPersonRepository.GetByCnhNumberAsync(deliveryPersonDto.CnhNumber);
            if (otherCnhDeliveryPerson is not null)
            {
                _logger.LogWarn(_messages.CnhAlreadyExists(otherCnhDeliveryPerson.Cnh.Number, otherCnhDeliveryPerson.Cnh.Type.ToString()));
                return Result<DeliveryPerson>.Failure(_messages.CnhAlreadyExists(otherCnhDeliveryPerson.Cnh.Number, otherCnhDeliveryPerson.Cnh.Type.ToString()));
            }

            var uploadImageResult = await UploadCnhImageAsync(deliveryPerson.Id, cnhImage);
            if (!uploadImageResult.IsSuccess)
            {
                _logger.LogWarn(uploadImageResult.ErrorMessage);
                return Result<DeliveryPerson>.Failure(uploadImageResult.ErrorMessage);
            }

            deliveryPerson.UpdateCnhImage(uploadImageResult.Url);

            await _deliveryPersonRepository.AddAsync(deliveryPerson);

            _memoryCache.Set(cacheKey, deliveryPerson, TimeSpan.FromMinutes(30));
            _logger.LogInfo(_messages.Registered(deliveryPerson.Id.ToString()));

            return Result<DeliveryPerson>.Success(deliveryPerson);
        }

        /// <summary>
        /// Asynchronously updates the CNH (National Driver's License) image for an existing delivery person.
        /// </summary>
        /// <param name="id">The unique identifier of the delivery person whose CNH image is to be updated.</param>
        /// <param name="cnhImage">An instance of <see cref="ImageFromFormFileDto"/> containing the new CNH image file name and data.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating the success or failure of the update operation. 
        /// Returns a successful result if the image was updated successfully; otherwise, returns a failure result with an error message.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// Updates the delivery person record with the new CNH image URL.
        /// Saves the updated delivery person record to the repository and logs the success of the image update.
        /// </remarks>
        public async Task<Result> UpdateDeliveryPersonAsync(Guid id, ImageFromFormFileDto cnhImage)
        {
            var deliveryPerson = await _deliveryPersonRepository.GetByIdAsync(id);
            if (deliveryPerson is null)
            {
                _logger.LogWarn(_messages.NotFound(id));
                return Result.Failure(_messages.NotFound(id));
            }

            var storageResult = await UploadCnhImageAsync(id, cnhImage);
            if (!storageResult.IsSuccess)
            {
                _logger.LogWarn(_messages.UploadImageGenericError(id));
                return Result.Failure(_messages.UploadImageGenericError(id));
            }

            deliveryPerson.UpdateCnhImage(storageResult.Url);

            await _deliveryPersonRepository.UpdateAsync(deliveryPerson);

            _memoryCache.Set($"DeliveryPerson_{deliveryPerson.Cnpj}", deliveryPerson, TimeSpan.FromMinutes(30));
            _logger.LogInfo(_messages.UploadImageSuccessfully(deliveryPerson.Cnh.Number, deliveryPerson.Cnh.Type.ToString()));

            return Result.Success();
        }
    }
}
