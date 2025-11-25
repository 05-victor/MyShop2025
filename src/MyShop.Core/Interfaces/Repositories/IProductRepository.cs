using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for product management
/// </summary>
public interface IProductRepository
{
    Task<Result<IEnumerable<Product>>> GetAllAsync();
    Task<Result<Product>> GetByIdAsync(Guid id);
    Task<Result<Product>> CreateAsync(Product product);
    Task<Result<Product>> UpdateAsync(Product product);
    Task<Result<bool>> DeleteAsync(Guid id);
}
