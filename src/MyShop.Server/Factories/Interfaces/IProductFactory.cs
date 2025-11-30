using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Factories.Interfaces
{
    /// <summary>
    /// Factory interface for creating Product entities from requests
    /// </summary>
    public interface IProductFactory
    {
        /// <summary>
        /// Create a new Product entity from a CreateProductRequest
        /// </summary>
        /// <param name="request">The product creation request containing product details</param>
        /// <returns>A new Product entity with initialized fields</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        Product Create(CreateProductRequest request);
    }
}
