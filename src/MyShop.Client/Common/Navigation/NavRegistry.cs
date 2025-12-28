using System;
using System.Collections.Generic;
using MyShop.Client.Views.Admin;
using MyShop.Client.Views.SalesAgent;
using MyShop.Client.Views.Customer;
using MyShop.Client.Views.Shared;

namespace MyShop.Client.Common.Navigation;

/// <summary>
/// Single source of truth for all navigation items across different shells.
/// Use this registry to maintain consistency and avoid duplication.
/// </summary>
public static class NavRegistry
{
    /// <summary>
    /// All navigation items with their role-based access control
    /// </summary>
    public static IReadOnlyList<NavItem> Items { get; } = new[]
    {
        // Dashboard - Different for each role
        new NavItem("dashboard", "Dashboard", typeof(AdminDashboardPage), "\uE80F", "Admin"),
        new NavItem("dashboard", "Dashboard", typeof(SalesAgentDashboardPage), "\uE80F", "Agent"),
        new NavItem("home", "Home", typeof(CustomerDashboardPage), "\uE80F", "Customer"),

        // Products
        new NavItem("products", "Products", typeof(AdminProductsPage), "\uE7B8", "Admin"),
        new NavItem("myProducts", "My Products", typeof(SalesAgentProductsPage), "\uE7B8", "Agent"),

        // Reports
        new NavItem("reports", "Reports", typeof(AdminReportsPage), "\uE9D9", "Admin"),
        new NavItem("reports", "Reports", typeof(SalesAgentReportsPage), "\uE9D9", "Agent"),

        // Users & Agents (Admin only)
        new NavItem("users", "Users", typeof(AdminUsersPage), "\uE716", "Admin"),
        new NavItem("salesAgents", "SalesAgents", typeof(AdminAgentRequestsPage), "\uE8F2", "Admin"),

        // Orders
        new NavItem("salesOrders", "Sales Orders", typeof(SalesAgentOrdersPage), "\uE7BF", "Agent"),
        new NavItem("purchaseOrders", "Purchase Orders", typeof(PurchaseOrdersPage), "\uE8F1", "Agent", "Customer"),

        // Earnings (Agent only)
        new NavItem("earnings", "Earnings", typeof(EarningsPage), "\uE7DB", "Agent"),

        // Shopping & Cart (Agent & Customer)
        new NavItem("shopping", "Shopping", typeof(ProductBrowsePage), "\uE719", "Agent", "Customer"),
        new NavItem("cart", "Cart", typeof(CartPage), "\uE7BF", "Agent", "Customer"),

        // Categories (Admin & Agent) - NEW!
        new NavItem("categories", "Categories", typeof(CategoriesPage), "\uE8D2", "Admin", "Agent"),

        // Common pages (All roles)
        new NavItem("profile", "Profile", typeof(ProfilePage), "\uE77B", "Admin", "Agent", "Customer"),
        new NavItem("settings", "Settings", typeof(SettingsPage), "\uE713", "Admin", "Agent", "Customer"),
    };

    /// <summary>
    /// Get navigation items filtered by role
    /// </summary>
    public static IEnumerable<NavItem> GetItemsForRole(string role)
    {
        return Items.Where(item => item.IsAccessibleBy(role));
    }

    /// <summary>
    /// Find page type by tag and role
    /// </summary>
    public static Type? GetPageType(string tag, string role)
    {
        return Items.FirstOrDefault(item => 
            item.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase) && 
            item.IsAccessibleBy(role))?.PageType;
    }

    /// <summary>
    /// Find navigation tag by page type
    /// </summary>
    public static string? GetTagByPageType(Type pageType)
    {
        return Items.FirstOrDefault(item => item.PageType == pageType)?.Tag;
    }
}
