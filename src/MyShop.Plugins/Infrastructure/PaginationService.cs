using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Diagnostics;

namespace MyShop.Plugins.Infrastructure;

/// <summary>
/// Implementation of IPaginationService.
/// Manages runtime pagination settings across the application.
/// Register as Singleton in DI.
/// </summary>
public class PaginationService : IPaginationService
{
    #region Constants (Defaults)

    private const int DEFAULT_PAGE_SIZE = 10;
    private const int MAX_PAGE_SIZE = 100;

    #endregion

    #region Private Fields

    private int _productsPageSize = DEFAULT_PAGE_SIZE;
    private int _ordersPageSize = DEFAULT_PAGE_SIZE;
    private int _customersPageSize = DEFAULT_PAGE_SIZE;
    private int _usersPageSize = DEFAULT_PAGE_SIZE;
    private int _agentRequestsPageSize = DEFAULT_PAGE_SIZE;
    private int _commissionsPageSize = DEFAULT_PAGE_SIZE;

    #endregion

    #region Properties

    public int DefaultPageSize => DEFAULT_PAGE_SIZE;

    public int MaxPageSize => MAX_PAGE_SIZE;

    public int ProductsPageSize
    {
        get => _productsPageSize;
        set => _productsPageSize = ClampPageSize(value);
    }

    public int OrdersPageSize
    {
        get => _ordersPageSize;
        set => _ordersPageSize = ClampPageSize(value);
    }

    public int CustomersPageSize
    {
        get => _customersPageSize;
        set => _customersPageSize = ClampPageSize(value);
    }

    public int UsersPageSize
    {
        get => _usersPageSize;
        set => _usersPageSize = ClampPageSize(value);
    }

    public int AgentRequestsPageSize
    {
        get => _agentRequestsPageSize;
        set => _agentRequestsPageSize = ClampPageSize(value);
    }

    public int CommissionsPageSize
    {
        get => _commissionsPageSize;
        set => _commissionsPageSize = ClampPageSize(value);
    }

    #endregion

    #region Methods

    public void Initialize(PaginationSettings settings)
    {
        ProductsPageSize = settings.ProductsPageSize;
        OrdersPageSize = settings.OrdersPageSize;
        CustomersPageSize = settings.CustomersPageSize;
        UsersPageSize = settings.UsersPageSize;
        AgentRequestsPageSize = settings.AgentRequestsPageSize;
        CommissionsPageSize = settings.CommissionsPageSize;

        Debug.WriteLine($"[PaginationService] Initialized: Products={ProductsPageSize}, Orders={OrdersPageSize}, Customers={CustomersPageSize}, Users={UsersPageSize}, AgentRequests={AgentRequestsPageSize}, Commissions={CommissionsPageSize}");
    }

    public void Reset()
    {
        _productsPageSize = DEFAULT_PAGE_SIZE;
        _ordersPageSize = DEFAULT_PAGE_SIZE;
        _customersPageSize = DEFAULT_PAGE_SIZE;
        _usersPageSize = DEFAULT_PAGE_SIZE;
        _agentRequestsPageSize = DEFAULT_PAGE_SIZE;
        _commissionsPageSize = DEFAULT_PAGE_SIZE;

        Debug.WriteLine("[PaginationService] Reset to defaults");
    }

    public int GetPageSize(PaginationEntityType entityType)
    {
        return entityType switch
        {
            PaginationEntityType.Products => ProductsPageSize,
            PaginationEntityType.Orders => OrdersPageSize,
            PaginationEntityType.Customers => CustomersPageSize,
            PaginationEntityType.Users => UsersPageSize,
            PaginationEntityType.AgentRequests => AgentRequestsPageSize,
            PaginationEntityType.Commissions => CommissionsPageSize,
            _ => DefaultPageSize
        };
    }

    public void SetPageSize(PaginationEntityType entityType, int pageSize)
    {
        var clampedSize = ClampPageSize(pageSize);

        switch (entityType)
        {
            case PaginationEntityType.Products:
                ProductsPageSize = clampedSize;
                break;
            case PaginationEntityType.Orders:
                OrdersPageSize = clampedSize;
                break;
            case PaginationEntityType.Customers:
                CustomersPageSize = clampedSize;
                break;
            case PaginationEntityType.Users:
                UsersPageSize = clampedSize;
                break;
            case PaginationEntityType.AgentRequests:
                AgentRequestsPageSize = clampedSize;
                break;
            case PaginationEntityType.Commissions:
                CommissionsPageSize = clampedSize;
                break;
        }

        Debug.WriteLine($"[PaginationService] Set {entityType} page size to {clampedSize}");
    }

    public PaginationSettings GetSettings()
    {
        return new PaginationSettings
        {
            DefaultPageSize = DefaultPageSize,
            ProductsPageSize = ProductsPageSize,
            OrdersPageSize = OrdersPageSize,
            CustomersPageSize = CustomersPageSize,
            UsersPageSize = UsersPageSize,
            AgentRequestsPageSize = AgentRequestsPageSize,
            CommissionsPageSize = CommissionsPageSize
        };
    }

    private int ClampPageSize(int value)
    {
        return Math.Clamp(value, 1, MAX_PAGE_SIZE);
    }

    #endregion
}
