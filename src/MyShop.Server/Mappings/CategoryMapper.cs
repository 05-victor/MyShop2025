
using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Mappings;

public class CategoryMapper
{
    public static CategoryResponse ToCategoryResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
        };
    }
}