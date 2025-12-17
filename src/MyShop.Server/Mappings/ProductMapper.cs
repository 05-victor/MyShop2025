using MyShop.Shared.DTOs.Responses;
using MyShop.Data.Entities;
using MyShop.Shared.Extensions;

namespace MyShop.Server.Mappings;

/// <summary>
/// Mapper for converting Product entities to ProductResponse DTOs
/// </summary>
public class ProductMapper
{
    /// <summary>
    /// Convert a Product entity to a ProductResponse DTO
    /// </summary>
    /// <param name="product">The product entity to convert</param>
    /// <returns>ProductResponse DTO with mapped data</returns>
    public static ProductResponse ToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Manufacturer = product.Manufacturer,
            DeviceType = product.DeviceType,
            ImportPrice = product.ImportPrice,
            SellingPrice = product.SellingPrice,
            Quantity = product.Quantity,
            CommissionRate = product.CommissionRate,
            Status = product.Status.ToApiString(),
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryName = product.Category?.Name,
            
            // Sale agent information
            SaleAgentId = product.SaleAgentId,
            SaleAgentUsername = product.SaleAgent?.Username,
            SaleAgentFullName = product.SaleAgent?.Profile?.FullName
        };
    }
}
