using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest createCategoryRequest);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest updateCategoryRequest);
    Task<bool> DeleteAsync(Guid id);
}