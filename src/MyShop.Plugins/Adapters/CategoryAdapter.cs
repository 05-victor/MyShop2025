using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for mapping Category DTOs to Category Models
/// Static class for stateless DTO-to-Model transformations
/// </summary>
public static class CategoryAdapter
{
    /// <summary>
    /// CategoryResponse (DTO) → Category (Model)
    /// </summary>
    public static Category ToModel(CategoryResponse dto)
    {
        return new Category
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// Convert list of CategoryResponse to list of Category
    /// </summary>
    public static List<Category> ToModelList(IEnumerable<CategoryResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }

    /// <summary>
    /// Category (Model) → CategoryResponse (DTO) - for reverse mapping if needed
    /// </summary>
    public static CategoryResponse ToDto(Category model)
    {
        return new CategoryResponse
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description
        };
    }
}
