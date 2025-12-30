using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

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
            UpdatedAt = dto.UpdatedAt,
            SaleAgentId = dto.SaleAgentId,
            SaleAgentUsername = dto.SaleAgentUsername,
            SaleAgentFullName = dto.SaleAgentFullName
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
            ImportPrice = model.ImportPrice,
            SellingPrice = model.SellingPrice,
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

    /// <summary>
    /// Product (Model) → CreateProductRequest (for bulk create)
    /// </summary>
    public static object ToCreateRequest(Product model)
    {
        // CategoryId is required by server - if null or empty, must provide a default or throw error
        Guid categoryId = model.CategoryId ?? Guid.Empty;
        
        // Validate that we have a valid category ID
        if (categoryId == Guid.Empty)
        {
            throw new InvalidOperationException($"Product '{model.Name}' must have a valid CategoryId. Please assign a category before creating.");
        }
        
        return new
        {
            sku = model.SKU,
            name = model.Name,
            manufacturer = model.Manufacturer,
            deviceType = model.DeviceType,
            importPrice = (int)model.ImportPrice,
            sellingPrice = (int)model.SellingPrice,
            quantity = model.Quantity,
            commissionRate = model.CommissionRate,
            status = model.Status ?? "AVAILABLE",
            description = model.Description,
            imageUrl = model.ImageUrl,
            categoryId = categoryId,
            saleAgentId = model.SaleAgentId == Guid.Empty ? (Guid?)null : model.SaleAgentId
        };
    }
}
