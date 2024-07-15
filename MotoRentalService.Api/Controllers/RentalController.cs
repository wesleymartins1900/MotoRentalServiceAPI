using Microsoft.AspNetCore.Mvc;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Api.Abstractions;
using MotoRentalService.Api.Attributtes;
using MotoRentalService.Domain.Enums;
using MotoRentalService.Application.Services.Interfaces;

namespace MotoRentalService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HasPermission(EUserRole.User)]
    public class RentalController(IRentalService rentalService) : ControllerBase
    {
        private readonly IRentalService _rentalService = rentalService;

        /// <summary>
        /// Creates a new motorcycle rental based on the provided data.
        /// </summary>
        /// <param name="rentalMotoDto">Object containing the details of the motorcycle rental.</param>
        /// <returns>
        /// Returns status 201 Created with the RentalMotoDto object in the response body if the rental is successfully created.
        /// Returns status 400 Bad Request if the provided data is invalid or if an error occurs during the creation of the rental.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRentalAsync([FromBody] RentalMotoDto rentalMotoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _rentalService.RegisterRentalMotoAsync(rentalMotoDto);
            if (result.IsSuccess)
                return CreatedAtAction("Sucesso", new { id = result.Value.Id }, rentalMotoDto);

            return BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Calculates the rental cost of a motorcycle based on the rental ID and the expected end date.
        /// </summary>
        /// <param name="id">Rental ID.</param>
        /// <param name="endDate">Expected end date of the rental.</param>
        /// <returns>
        /// Returns status 200 OK with the calculated rental cost in the response body if the calculation is successful.
        /// Returns status 400 Bad Request if the provided data is invalid or if an error occurs during the rental cost calculation.
        /// </returns>
        [HttpPost(ApiRoutes.Rental.CalculateValue)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateRentalValue([FromQuery] Guid id, [FromBody] DateTime endDate)
        {
            var result = await _rentalService.RentalValueCalculateAsync(id, endDate);
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }
    }
}
