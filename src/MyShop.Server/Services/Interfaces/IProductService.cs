using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(CreateProductRequest createProductRequest);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest updateProductRequest);
    Task<bool> DeleteAsync(Guid id);
}