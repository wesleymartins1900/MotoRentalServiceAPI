using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MotoRentalService.Api.Abstractions;
using MotoRentalService.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MotoRentalService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        private string GenerateJwtToken(string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a quick token for API validation.
        /// </summary>
        /// <param name="role">Defines the role as "admin" or "user".</param>
        /// <returns>
        /// Returns status 200 OK with the calculated token in the response body if the token generation is successful.
        /// </returns>
        [HttpGet(ApiRoutes.Auth)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GenerateToken([FromRoute] string role)
        {
            Enum.TryParse<EUserRole>(role, true, out var userRole);
            
            if (!Enum.IsDefined(typeof(EUserRole), userRole))
                return Unauthorized();

            var token = GenerateJwtToken(userRole.ToString());

            return Ok(new { token });
        }
    }
}