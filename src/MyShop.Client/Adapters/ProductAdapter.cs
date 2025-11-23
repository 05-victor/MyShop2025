using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Client.Adapters;

/// <summary>
/// Adapter for mapping Product DTOs to Product Models
/// Static class for stateless DTO-to-Model transformations
/// </summary>
public static class ProductAdapter
{
    /// <summary>
    /// ProductResponse (DTO) → Product (Model)
    /// </summary>
    public static Product ToModel(ProductResponse dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            SKU = dto.SKU,
            Description = dto.Description ?? string.Empty,
            ImageUrl = dto.ImageUrl,
            ImportPrice = dto.ImportPrice ?? 0,
            SellingPrice = dto.SellingPrice ?? 0,
            Quantity = dto.Quantity ?? 0,
            CategoryName = dto.CategoryName,
            Manufacturer = dto.Manufacturer,
            DeviceType = dto.DeviceType,
            CommissionRate = dto.CommissionRate ?? 0,
            Status = dto.Status ?? "AVAILABLE",
            CreatedAt = dto.CreatedAt ?? DateTime.Now,
            UpdatedAt = dto.UpdatedAt
        };
    }

    /// <summary>
    /// Convert list of ProductResponse to list of Product
    /// </summary>
    public static List<Product> ToModelList(IEnumerable<ProductResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }

    /// <summary>
    /// Product (Model) → ProductResponse (DTO) - for reverse mapping if needed
    /// </summary>
    public static ProductResponse ToDto(Product model)
    {
        return new ProductResponse
        {
            Id = model.Id,
            Name = model.Name,
            SKU = model.SKU,
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            ImportPrice = (int)model.ImportPrice,
            SellingPrice = (int)model.SellingPrice,
            Quantity = model.Quantity,
            CategoryName = model.CategoryName,
            Manufacturer = model.Manufacturer,
            DeviceType = model.DeviceType,
            CommissionRate = model.CommissionRate,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}
