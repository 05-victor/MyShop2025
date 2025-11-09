//using AutoMapper;
//using MyShop.Data.Entities;
//using MyShop.Shared.DTOs.Requests;

//namespace MyShop.Server.EntityMappings
//{
//    public class ProductProfile : AutoMapper.Profile
//    {
//        public ProductProfile()
//        {
//            // Cấu hình map giữa UpdateProductRequest và Product
//            CreateMap<UpdateProductRequest, Product>()
//                .ForMember(dest => dest.Category, opt => opt.Ignore()) // 💥 tránh map navigation
//                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

//        }
//    }
//}
