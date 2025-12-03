using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades.Products;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Categories management with server-side paging
/// Extends PagedViewModelBase to inherit paging logic
/// </summary>
public partial class AdminCategoriesViewModel : PagedViewModelBase<CategoryViewModel>
{
    private readonly ICategoryFacade _categoryFacade;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _sortBy = "name";

    [ObservableProperty]
    private bool _sortDescending = false;

    public AdminCategoriesViewModel(
        ICategoryFacade categoryFacade,
        IToastService toastService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(toastService, navigationService)
    {
        _categoryFacade = categoryFacade;
        _dialogService = dialogService;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Reload when sort options change
    /// </summary>
    partial void OnSortByChanged(string value)
    {
        _ = LoadPageAsync();
    }

    partial void OnSortDescendingChanged(bool value)
    {
        _ = LoadPageAsync();
    }

    /// <summary>
    /// Override LoadPageAsync to fetch categories with server-side paging
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        SetLoadingState(true);
        try
        {
            var result = await _categoryFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load categories");
                Items.Clear();
                UpdatePagingInfo(0);
                return;
            }

            var categories = result.Data;
            Items.Clear();
            
            // Apply client-side filtering
            var filtered = categories.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                filtered = filtered.Where(c => c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply client-side paging
            var paged = filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            
            foreach (var category in paged)
            {
                Items.Add(CategoryViewModel.FromModel(category));
            }

            UpdatePagingInfo(filtered.Count());

            System.Diagnostics.Debug.WriteLine($"[AdminCategoriesViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminCategoriesViewModel] Error loading categories: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading categories: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        try
        {
            var inputResult = await _dialogService.ShowInputAsync(
                "Add Category",
                "Enter category name:",
                "Category name");

            if (!inputResult.IsSuccess || string.IsNullOrWhiteSpace(inputResult.Data))
                return;

            var categoryName = inputResult.Data;

            var descriptionResult = await _dialogService.ShowInputAsync(
                "Add Category",
                "Enter category description (optional):",
                "Description");

            var description = descriptionResult.IsSuccess ? descriptionResult.Data : string.Empty;

            var result = await _categoryFacade.CreateCategoryAsync(categoryName, description);
            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Category '{categoryName}' created successfully");
                await RefreshAsync();
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to create category");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error creating category: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditCategoryAsync(CategoryViewModel category)
    {
        try
        {
            var nameResult = await _dialogService.ShowInputAsync(
                "Edit Category",
                "Enter new category name:",
                category.Name);

            if (!nameResult.IsSuccess || string.IsNullOrWhiteSpace(nameResult.Data))
                return;

            var newName = nameResult.Data;

            var descriptionResult = await _dialogService.ShowInputAsync(
                "Edit Category",
                "Enter new description (optional):",
                category.Description ?? string.Empty);

            var newDescription = descriptionResult.IsSuccess ? descriptionResult.Data : category.Description;

            var result = await _categoryFacade.UpdateCategoryAsync(category.Id, newName, newDescription ?? string.Empty);
            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Category '{newName}' updated successfully");
                category.Name = newName;
                category.Description = newDescription ?? string.Empty;
                category.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to update category");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error updating category: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryViewModel category)
    {
        try
        {
            var productCountResult = await _categoryFacade.GetProductCountByCategoryAsync();
            var productCount = 0;
            if (productCountResult.IsSuccess && productCountResult.Data != null && productCountResult.Data.TryGetValue(category.Name, out var count))
            {
                productCount = count;
            }

            var message = productCount > 0
                ? $"Category '{category.Name}' has {productCount} product(s). Are you sure you want to delete it?"
                : $"Are you sure you want to delete category '{category.Name}'?";

            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Delete Category",
                message);

            if (!confirmResult.IsSuccess || !confirmResult.Data)
                return;

            var result = await _categoryFacade.DeleteCategoryAsync(category.Id);
            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Category '{category.Name}' deleted successfully");
                Items.Remove(category);
                UpdatePagingInfo(TotalItems - 1);
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to delete category");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error deleting category: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewProductsAsync(CategoryViewModel category)
    {
        // TODO: Navigate to products page filtered by category
        await _toastHelper?.ShowInfo($"Viewing products in '{category.Name}' category");
        System.Diagnostics.Debug.WriteLine($"[AdminCategoriesViewModel] View products in category: {category.Name}");
    }
}

/// <summary>
/// ViewModel representation of Category for UI binding
/// </summary>
public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _updatedAt;

    [ObservableProperty]
    private int _productCount;

    public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");
    public string UpdatedAtFormatted => UpdatedAt?.ToString("MMM dd, yyyy") ?? "-";

    public static CategoryViewModel FromModel(Category category)
    {
        return new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description ?? string.Empty,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ProductCount = 0 // Will be loaded separately if needed
        };
    }

    public Category ToModel()
    {
        return new Category
        {
            Id = Id,
            Name = Name,
            Description = Description,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
