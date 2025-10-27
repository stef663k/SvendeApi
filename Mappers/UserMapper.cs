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
        CreateMap<UserModel, UserDTO>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.RoleName).ToList()));

        CreateMap<CreateUserDTO, UserModel>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => new Collection<UserRole>())); // Initialize empty collection

        CreateMap<UpdateUserDTO, UserModel>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UserModel, AuthResponseDTO>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));
    }
}

public static class UserMapperExtensions
{
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