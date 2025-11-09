using MyShop.Data.Entities;
using MyShop.Server.Factories;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Factories
{
    public class ProductFactory : BaseFactory<Product, CreateProductRequest>
    {
        public override Product Create(CreateProductRequest request)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Product name cannot be empty.", nameof(request.Name));

            if (request.SellingPrice < request.ImportPrice)
                throw new ArgumentException("Selling price cannot be less than import price.");

            if (request.CommissionRate < 0 || request.CommissionRate > 1)
                throw new ArgumentException("Commission rate must be between 0 and 1.");

            // Initialize new Product entity
            // Only have to set CategoryId here; Category navigation property will be set in service layer (or automatically set?)
            var product = new Product
            {
                SKU = request.SKU.Trim(),
                Name = request.Name.Trim(),
                Manufacturer = request.Manufacturer?.Trim(),
                DeviceType = request.DeviceType?.Trim(),
                ImportPrice = request.ImportPrice,
                SellingPrice = request.SellingPrice,
                Quantity = request.Quantity,
                CommissionRate = request.CommissionRate,
                Status = request.Status != null ? request.Status.Trim() : "AVAILABLE",
                Description = request.Description?.Trim(),
                ImageUrl = request.ImageUrl?.Trim(),
                CategoryId = request.CategoryId
            };

            // Set additional fields
            AssignNewId(product);
            SetAuditFields(product);
            
            return product;
        }
    }
}
