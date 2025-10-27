using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;

public class LikeMapper : Profile
{
    public LikeMapper()
    {
        CreateMap<LikeModel, LikeDTO>();
    }
}