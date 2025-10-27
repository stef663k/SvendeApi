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
        // Mapping fra RoleModel til RoleDTO, mapping af alle properties
        CreateMap<RoleModel, RoleDTO>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName));

        // Mapping fra CreateRoleDTO til RoleModel, genererer nyt GUID og initialiserer tomme collections
        CreateMap<CreateRoleDTO, RoleModel>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => new Collection<UserRole>()));

        // Mapping fra UpdateRoleDTO til RoleModel, ignorerer RoleId og UserRoles da disse ikke skal opdateres
        CreateMap<UpdateRoleDTO, RoleModel>()
            .ForMember(dest => dest.RoleId, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
    }
}

// Extension metoder for RoleModel mapping, alternativ til AutoMapper
public static class RoleMapperExtensions
{
    // Konverterer RoleModel til RoleDTO, manuel mapping
    public static RoleDTO ToRoleDTO(this RoleModel role)
    {
        return new RoleDTO
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
        };
    }

    // Konverterer CreateRoleDTO til RoleModel, genererer nyt GUID og initialiserer tomme collections
    public static RoleModel ToRoleModel(this CreateRoleDTO dto)
    {
        return new RoleModel
        {
            RoleId = Guid.NewGuid(),
            RoleName = dto.RoleName,
            UserRoles = new Collection<UserRole>(),
        };
    }

    // Opdaterer RoleModel med værdier fra UpdateRoleDTO, opdaterer kun hvis værdi ikke er null
    public static void UpdateFromDTO(this RoleModel role, UpdateRoleDTO dto)
    {
        if (!string.IsNullOrEmpty(dto.RoleName))
            role.RoleName = dto.RoleName;
    }

    // Returnerer liste af tilgængelige rollenavne i systemet
    public static List<string> GetAvailableRoleNames()
    {
        return new List<string> {"Admin", "User", "Moderator"};
    }
}