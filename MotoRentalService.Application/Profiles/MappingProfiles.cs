using AutoMapper;
using MotoRentalService.Application.Dtos;
using MotoRentalService.Domain.Entities;
using MotoRentalService.Domain.ValueObjects;

namespace MotoRentalService.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDeliveryPersonDto, DeliveryPerson>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => new Cnpj(src.Cnpj)))
                .ForMember(dest => dest.Cnh, opt => opt.MapFrom(src => new Cnh(src.CnhNumber, src.CnhType)));

            CreateMap<RegisterMotoDto, Moto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Deleted, opt => opt.Ignore());
        }
    }
}
