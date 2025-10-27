using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        // Mapping fra UserModel til UserDTO, konverterer UserRoles til en liste af role navne
        CreateMap<UserModel, UserDTO>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.RoleName).ToList()));

        // Mapping fra CreateUserDTO til UserModel, genererer nyt GUID og initialiserer tomme collections
        CreateMap<CreateUserDTO, UserModel>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => new Collection<UserRole>()));

        // Mapping fra UpdateUserDTO til UserModel, opdaterer kun hvis værdi ikke er null
        CreateMap<UpdateUserDTO, UserModel>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Mapping fra UserModel til AuthResponseDTO, ignorerer Token og ExpiresAt da disse genereres i service
        CreateMap<UserModel, AuthResponseDTO>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
    }
}

// Extension metoder for UserModel mapping, alternativ til AutoMapper
public static class UserMapperExtensions
{
    // Konverterer UserModel til UserDTO, manuel mapping
    public static UserDTO ToUserDTO(this UserModel user)
    {
        return new UserDTO
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = user.UserRoles?.Select(ur => ur.Role.RoleName).ToList() ?? new List<string>(),
        };
    }

    // Konverterer CreateUserDTO til UserModel, genererer nyt GUID og initialiserer tomme collections
    public static UserModel ToUserModel(this CreateUserDTO createUserDTO)
    {
        return new UserModel
        {
            UserId = Guid.NewGuid(),
            FirstName = createUserDTO.FirstName,
            LastName = createUserDTO.LastName,
            Email = createUserDTO.Email,
            Password = createUserDTO.Password,
            UserRoles = new Collection<UserRole>(),
        };
    }

    // Opdaterer UserModel med værdier fra UpdateUserDTO, opdaterer kun hvis værdi ikke er null
    public static void UpdateFromDTO(this UserModel user, UpdateUserDTO dto)
    {
        if(!string.IsNullOrEmpty(dto.FirstName))
            user.FirstName = dto.FirstName;
        if(!string.IsNullOrEmpty(dto.LastName))
            user.LastName = dto.LastName;
        if(!string.IsNullOrEmpty(dto.Email))
            user.Email = dto.Email;
    }
}