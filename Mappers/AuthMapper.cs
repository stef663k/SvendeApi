using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;


public class AuthMapper : Profile
{
    public AuthMapper()
    {
        // Mapping fra UserModel til AuthResponseDTO, ignorerer Token og RefreshToken da disse genereres i service
        CreateMap<UserModel, AuthResponseDTO>()
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Ignorer da dette genereres i auth service
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore()) // Ignorer da dette genereres i auth service
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
    }
}