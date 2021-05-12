using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductToReturnDto>()
                //instead of getting a string of ProductBrand class, get the Name field of ProductBrand
                //same as ProductType
                .ForMember(d => d.ProductBrand, o => o.MapFrom(s => s.ProductBrand.Name))
                .ForMember(d => d.ProductType, o => o.MapFrom(s => s.ProductType.Name))
                //get picture url from producturlresolver class
                .ForMember(d => d.PictureUrl, o => o.MapFrom<ProductUrlResolver>());
            
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>();

        }
    }
}