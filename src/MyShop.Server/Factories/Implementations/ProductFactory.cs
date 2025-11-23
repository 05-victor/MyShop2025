using MyShop.Data.Entities;
using MyShop.Server.Factories.Interfaces;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Factories.Implementations
{
    /// <summary>
    /// Factory for creating Product entities from CreateProductRequest
    /// </summary>
    public class ProductFactory : BaseFactory<Product, CreateProductRequest>, IProductFactory
    {
        /// <summary>
        /// Create a new Product entity from a CreateProductRequest
        /// </summary>
        /// <param name="request">The product creation request</param>
        /// <returns>A new Product entity with initialized fields</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails</exception>
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
<<<<<<< HEAD:src/MyShop.Server/Factories/Implementations/ProductFactory.cs
=======
            // Set Category to a non-null value to satisfy required member
>>>>>>> master:src/MyShop.Server/Factories/ProductFactory.cs
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
                CategoryId = request.CategoryId,
<<<<<<< HEAD:src/MyShop.Server/Factories/Implementations/ProductFactory.cs
                SaleAgentId = request.SaleAgentId // Will be set in service if null
=======
                Category = null! // Will be set in the service layer or by EF Core
>>>>>>> master:src/MyShop.Server/Factories/ProductFactory.cs
            };

            // Set additional fields
            AssignNewId(product);
            SetAuditFields(product);
            
            return product;
        }
    }
}
