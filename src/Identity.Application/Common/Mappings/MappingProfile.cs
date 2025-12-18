using AutoMapper;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());
    }
}