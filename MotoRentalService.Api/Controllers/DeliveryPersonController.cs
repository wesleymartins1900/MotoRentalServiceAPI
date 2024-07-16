using Microsoft.AspNetCore.Mvc;
using MotoRentalService.Api.Attributtes;
using MotoRentalService.Domain.Enums;
using MotoRentalService.Api.Abstractions;
using MotoRentalService.Application.Dtos;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Application.Services.Interfaces;
using MotoRentalService.Api.Abstractions.Dtos;
using MotoRentalService.Api.Attributes;

namespace MotoRentalService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HasPermission(EUserRole.User)]
    public class DeliveryPersonController(IDeliveryPersonService deliveryPersonService) : ControllerBase
    {
        private readonly IDeliveryPersonService _deliveryPersonService = deliveryPersonService;

        /// <summary>
        /// Converts an IFormFile image to a byte array and encapsulates it in an ImageFromFormFileDto.
        /// Validates the image extension to ensure it is either PNG or BMP.
        /// </summary>
        /// <param name="image">The image file to be converted.</param>
        /// <returns>
        /// A Result object containing an ImageFromFormFileDto with the image data and file name
        /// if successful; otherwise, a failure result with an error message.
        /// </returns>
        
        [ExtensionValidatorFilter(["image/png", "image/bmp"])]
        private async Task<Result<ImageFromFormFileDto>> ConvertImageFromFormFile(IFormFile image)
        {
            byte[]? imageToArray = null;
            using (var stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);
                imageToArray = stream.ToArray();
            }

            if (imageToArray is null)
                return Result<ImageFromFormFileDto>.Failure("Invalid image.");

            var result = new ImageFromFormFileDto
            {
                Data = imageToArray,
                FileName = image.FileName
            };

            return Result<ImageFromFormFileDto>.Success(result);
        }

        /// <summary>
        /// Registers a new delivery person based on the provided data.
        /// </summary>
        /// <param name="registerDeliveryPersonDto">Object containing the details of the delivery person to be registered.</param>
        /// <returns>
        /// Returns status 201 Created with the delivery person object in the response body if the registration is successful.
        /// Returns status 400 Bad Request if the provided data is invalid or if an error occurs during the registration of the delivery person.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterDeliveryPersonAsync([FromForm] RegisterDeliveryPersonWithImageDto registerDeliveryPersonDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var imageDto = await ConvertImageFromFormFile(registerDeliveryPersonDto.CnhImage);
                if (!imageDto.IsSuccess)
                    return BadRequest(imageDto.ErrorMessage);

                var result = await _deliveryPersonService.RegisterDeliveryPersonAsync(registerDeliveryPersonDto, imageDto.Value);
                if (result.IsSuccess)
                    return Created(nameof(RegisterDeliveryPersonAsync), new { id = result.Value.Id.ToString() });

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Updates the National Driver's License (CNH) image of a delivery person based on the provided ID.
        /// </summary>
        /// <param name="id">ID of the delivery person.</param>
        /// <param name="cnhImage">Image file of the CNH to be updated (supported formats: PNG, BMP).</param>
        /// <returns>
        /// Returns status 204 No Content if the CNH image update is successful.
        /// Returns status 400 Bad Request if the image file extension is invalid or if an error occurs during the update.
        /// </returns>
        [HttpPost(ApiRoutes.DeliveryPerson.UpdateCnh)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCnhImageAsync([FromForm] UpdateDeliveryPersonWithImageDto updateDeliveryPersonDto)
        {
            try
            {
                var imageDto = await ConvertImageFromFormFile(updateDeliveryPersonDto.CnhImage);
                if (!imageDto.IsSuccess)
                    return BadRequest(imageDto.ErrorMessage);

                var result = await _deliveryPersonService.UpdateDeliveryPersonAsync(updateDeliveryPersonDto.Id, imageDto.Value);
                if (result.IsSuccess)
                    return NoContent();

                return BadRequest(result.ErrorMessage);
            } 
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}