using MotoRentalService.Application.Dtos;
using MotoRentalService.CrossCutting.Primitives;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.Shared;

namespace MotoRentalService.Application.Services.Interfaces
{
    public interface IMotoService
    {
        Task<Result<Moto>> RegisterMotoAsync(RegisterMotoDto motoDto);
        Task<PagedResult<Moto>> GetMotosAsync(string? plate = null, int pageNumber = 1, int pageSize = 10);
        Task<Result<Moto>> UpdateMotoAsync(Guid id, UpdateMotoDto motoDto);
        Task<Result> DeleteMotoAsync(Guid id);
    }
}
