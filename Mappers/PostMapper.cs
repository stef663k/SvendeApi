using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SvendeApi.DTO;
using SvendeApi.Models;

namespace SvendeApi.Mappers;

public class PostMapper : Profile
{
    public PostMapper()
    {
        CreateMap<PostModel, PostDTO>();

        CreateMap<CreatePostDTO, PostModel>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(d => d.AuthorId, opt => opt.Ignore())
            .ForMember(d => d.Author, opt => opt.Ignore())
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => false));
    }
}