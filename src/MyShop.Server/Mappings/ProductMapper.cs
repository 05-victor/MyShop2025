using MyShop.Shared.DTOs.Responses;

using MyShop.Data.Entities;

namespace MyShop.Server.Mappings;
public class ProductMapper
{
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
            Status = product.Status,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryName = product.Category?.Name
        };
    }
}
