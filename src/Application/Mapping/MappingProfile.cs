using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Item, ItemDto>();
        CreateMap<CreateItemDto, Item>();
        CreateMap<UpdateItemDto, Item>();
    }
}
