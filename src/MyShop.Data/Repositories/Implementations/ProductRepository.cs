using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.Extensions;

namespace MyShop.Data.Repositories.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly ShopContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ShopContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile)
            .ToListAsync();
    }

    public async Task<PagedResult<Product>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();

        if (product.SaleAgentId.HasValue)
        {
            await _context.Entry(product)
                .Reference(p => p.SaleAgent)
                .LoadAsync();

            if (product.SaleAgent != null)
            {
                await _context.Entry(product.SaleAgent)
                    .Reference(u => u.Profile)
                    .LoadAsync();
            }
        }

        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Product>> SearchAsync(
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
        int pageSize = 20)
    {
        _logger.LogInformation(
            "Searching products: Query={Query}, CategoryId={CategoryId}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, " +
            "InStockOnly={InStockOnly}, Manufacturer={Manufacturer}, DeviceType={DeviceType}, Status={Status}, " +
            "SaleAgentId={SaleAgentId}, MinStock={MinStock}, MaxStock={MaxStock}, " +
            "MinCommissionRate={MinCommissionRate}, MaxCommissionRate={MaxCommissionRate}, " +
            "SortBy={SortBy}, SortOrder={SortOrder}, Page={Page}, PageSize={PageSize}",
            query, categoryId, minPrice, maxPrice, inStockOnly, manufacturer, deviceType, status,
            saleAgentId, minStock, maxStock, minCommissionRate, maxCommissionRate,
            sortBy, sortOrder, pageNumber, pageSize);

        var queryable = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile)
            .AsNoTracking();

        // Apply text search filter
        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchTerm = query.ToLower();
            queryable = queryable.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.SKU != null && p.SKU.ToLower().Contains(searchTerm)) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(searchTerm)) ||
                (p.DeviceType != null && p.DeviceType.ToLower().Contains(searchTerm)));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            queryable = queryable.Where(p => p.CategoryId == categoryId.Value);
        }

        // Apply price range filters
        if (minPrice.HasValue)
        {
            queryable = queryable.Where(p => p.SellingPrice >= (int)minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.SellingPrice <= (int)maxPrice.Value);
        }

        // Apply stock filters
        if (inStockOnly == true)
        {
            queryable = queryable.Where(p => p.Quantity > 0);
        }

        if (minStock.HasValue)
        {
            queryable = queryable.Where(p => p.Quantity >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            queryable = queryable.Where(p => p.Quantity <= maxStock.Value);
        }

        // Apply manufacturer filter
        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            queryable = queryable.Where(p => p.Manufacturer != null && 
                p.Manufacturer.ToLower().Contains(manufacturer.ToLower()));
        }

        // Apply device type filter
        if (!string.IsNullOrWhiteSpace(deviceType))
        {
            queryable = queryable.Where(p => p.DeviceType != null && 
                p.DeviceType.ToLower().Contains(deviceType.ToLower()));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            var productStatus = StatusEnumExtensions.ParseApiString<MyShop.Shared.Enums.ProductStatus>(status);
            queryable = queryable.Where(p => p.Status == productStatus);
        }

        // Apply sale agent filter
        if (saleAgentId.HasValue)
        {
            queryable = queryable.Where(p => p.SaleAgentId == saleAgentId.Value);
        }

        // Apply commission rate filters
        if (minCommissionRate.HasValue)
        {
            queryable = queryable.Where(p => p.CommissionRate >= minCommissionRate.Value);
        }

        if (maxCommissionRate.HasValue)
        {
            queryable = queryable.Where(p => p.CommissionRate <= maxCommissionRate.Value);
        }

        // Apply sorting
        var isDescending = sortOrder?.ToLower() == "desc";
        queryable = sortBy?.ToLower() switch
        {
            "name" => isDescending 
                ? queryable.OrderByDescending(p => p.Name) 
                : queryable.OrderBy(p => p.Name),
            "price" or "sellingprice" => isDescending 
                ? queryable.OrderByDescending(p => p.SellingPrice) 
                : queryable.OrderBy(p => p.SellingPrice),
            "stock" or "quantity" => isDescending 
                ? queryable.OrderByDescending(p => p.Quantity) 
                : queryable.OrderBy(p => p.Quantity),
            "importprice" => isDescending 
                ? queryable.OrderByDescending(p => p.ImportPrice) 
                : queryable.OrderBy(p => p.ImportPrice),
            "commission" or "commissionrate" => isDescending 
                ? queryable.OrderByDescending(p => p.CommissionRate) 
                : queryable.OrderBy(p => p.CommissionRate),
            "manufacturer" => isDescending 
                ? queryable.OrderByDescending(p => p.Manufacturer) 
                : queryable.OrderBy(p => p.Manufacturer),
            "devicetype" => isDescending 
                ? queryable.OrderByDescending(p => p.DeviceType) 
                : queryable.OrderBy(p => p.DeviceType),
            "status" => isDescending 
                ? queryable.OrderByDescending(p => p.Status) 
                : queryable.OrderBy(p => p.Status),
            "createdat" or "created" => isDescending 
                ? queryable.OrderByDescending(p => p.CreatedAt) 
                : queryable.OrderBy(p => p.CreatedAt),
            "updatedat" or "updated" => isDescending 
                ? queryable.OrderByDescending(p => p.UpdatedAt) 
                : queryable.OrderBy(p => p.UpdatedAt),
            _ => isDescending 
                ? queryable.OrderByDescending(p => p.CreatedAt) 
                : queryable.OrderBy(p => p.CreatedAt)
        };

        // Get total count
        var totalCount = await queryable.CountAsync();

        // Apply pagination
        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation(
            "Found {Count} products (page {Page}/{TotalPages}, total: {TotalCount})",
            items.Count, pageNumber, (int)Math.Ceiling((double)totalCount / pageSize), totalCount);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
