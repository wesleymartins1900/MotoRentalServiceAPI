using Microsoft.AspNetCore.Mvc;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Api.Abstractions;
using MotoRentalService.Api.Attributtes;
using MotoRentalService.Domain.Enums;
using MotoRentalService.Application.Services.Interfaces;

namespace MotoRentalService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [HasPermission(EUserRole.Admin)]
    public class MotoController(IMotoService motoService) : ControllerBase
    {
        private readonly IMotoService _motoService = motoService;

        /// <summary>
        /// Registers a new motorcycle based on the provided data.
        /// </summary>
        /// <param name="motoDto">Object containing the details of the motorcycle to be registered.</param>
        /// <returns>
        /// Returns status 200 OK with the registered motorcycle object in the response body if the registration is successful.
        /// Returns status 400 Bad Request if the provided data is invalid or if an error occurs during the motorcycle registration.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterMotoAsync([FromBody] RegisterMotoDto motoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _motoService.RegisterMotoAsync(motoDto);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Created(nameof(RegisterMotoAsync), new { id = result.Value.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Retrieves information about motorcycles based on the provided license plate.
        /// </summary>
        /// <param name="plate">License plate of the motorcycle to search for.</param>
        /// <returns>
        /// Returns status 200 OK with the list of motorcycles matching the specified license plate in the response body if found.
        /// Returns status 404 Not Found if no motorcycles match the specified license plate.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMotosAsync([FromQuery] string? plate = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedResult = await _motoService.GetMotosAsync(plate, pageNumber, pageSize);
                if (pagedResult.Items.Count is 0)
                    return NotFound();

                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Updates the details of an existing motorcycle based on the provided ID.
        /// </summary>
        /// <param name="id">ID of the motorcycle to be updated.</param>
        /// <param name="motoDto">Object containing the new details of the motorcycle.</param>
        /// <returns>
        /// Returns status 200 OK with the updated motorcycle object in the response body if the update is successful.
        /// Returns status 400 Bad Request if the provided data is invalid or if an error occurs during the motorcycle update.
        /// </returns>
        [HttpPut(ApiRoutes.BaseWithGuidId)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMotoAsync([FromRoute] Guid id, [FromBody] UpdateMotoDto motoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _motoService.UpdateMotoAsync(id, motoDto);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok(result.Value);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Deletes a motorcycle based on the provided ID.
        /// </summary>
        /// <param name="id">ID of the motorcycle to be deleted.</param>
        /// <returns>
        /// Returns status 200 OK if the motorcycle is successfully deleted.
        /// Returns status 400 Bad Request if an error occurs during the deletion of the motorcycle.
        /// </returns>
        [HttpDelete(ApiRoutes.BaseWithGuidId)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMotoAsync([FromRoute] Guid id)
        {
            try
            {
                var result = await _motoService.DeleteMotoAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return Ok();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex);
            }
        }
    }
}
