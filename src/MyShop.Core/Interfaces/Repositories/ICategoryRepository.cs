using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for category management
/// </summary>
public interface ICategoryRepository
{
    Task<Result<IEnumerable<Category>>> GetAllAsync();
    Task<Result<Category>> GetByIdAsync(Guid id);
    Task<Result<Category>> CreateAsync(Category category);
    Task<Result<Category>> UpdateAsync(Category category);
    Task<Result<bool>> DeleteAsync(Guid id);
    
    /// <summary>
    /// Get paginated categories with search and sorting
    /// </summary>
    Task<Result<PagedList<Category>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize,
        string? searchQuery = null,
        string sortBy = "name",
        bool sortDescending = false);
}
