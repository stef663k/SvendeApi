using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;

public class FollowMapper : Profile
{
    public FollowMapper()
    {
        // Mapping fra FollowerModel til FollowDTO, mapping af alle properties
        CreateMap<FollowerModel, FollowDTO>();
    }
}