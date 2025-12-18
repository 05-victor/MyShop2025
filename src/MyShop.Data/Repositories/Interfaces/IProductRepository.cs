using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<PagedResult<Product>> GetAllAsync(int pageNumber, int pageSize);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Search products with advanced filtering, sorting, and pagination
    /// </summary>
    Task<PagedResult<Product>> SearchAsync(
        string? query = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStockOnly = null,
        string? manufacturer = null,
        string? deviceType = null,
        string? status = null,
        Guid? saleAgentId = null,
        int? minStock = null,
        int? maxStock = null,
        double? minCommissionRate = null,
        double? maxCommissionRate = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        int pageNumber = 1,
        int pageSize = 20);
}
