using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;

public class RoleMapper : Profile
{
    public RoleMapper()
    {
        CreateMap<RoleModel, RoleDTO>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName));

        CreateMap<CreateRoleDTO, RoleModel>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => new Collection<UserRole>()));

        CreateMap<UpdateRoleDTO, RoleModel>()
            .ForMember(dest => dest.RoleId, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
    }
}

public static class RoleMapperExtensions
{
    public static RoleDTO ToRoleDTO(this RoleModel role)
    {
        return new RoleDTO
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
        };
    }

    public static RoleModel ToRoleModel(this CreateRoleDTO dto)
    {
        return new RoleModel
        {
            RoleId = Guid.NewGuid(),
            RoleName = dto.RoleName,
            UserRoles = new Collection<UserRole>(),
        };
    }

    public static void UpdateFromDTO(this RoleModel role, UpdateRoleDTO dto)
    {
        if (!string.IsNullOrEmpty(dto.RoleName))
            role.RoleName = dto.RoleName;
    }

    public static List<string> GetAvailableRoleNames()
    {
        return new List<string> {"Admin", "User", "Moderator"};
    }
}