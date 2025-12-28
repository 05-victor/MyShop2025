using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Facades;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Categories page (used by both Admin and Agent)
/// </summary>
public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryFacade _categoryFacade;
    private readonly MyShop.Core.Interfaces.Services.IDialogService _dialogService;
    private readonly MyShop.Core.Interfaces.Services.IToastService _toastService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private ObservableCollection<CategoryResponse> _categories = new();

    [ObservableProperty]
    private ObservableCollection<CategoryResponse> _filteredCategories = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _selectedSortOption = "Name A-Z";

    [ObservableProperty]
    private bool _hasCategories = false;

    [ObservableProperty]
    private bool _hasFilteredCategories = false;

    [ObservableProperty]
    private bool _showNoCategoriesState = false;

    [ObservableProperty]
    private bool _showNoResultsState = false;

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand DeleteCategoryCommand { get; }

    public CategoriesViewModel(ICategoryFacade categoryFacade, MyShop.Core.Interfaces.Services.IDialogService dialogService, MyShop.Core.Interfaces.Services.IToastService toastService)
    {
        _categoryFacade = categoryFacade;
        _dialogService = dialogService;
        _toastService = toastService;

        LoadCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        RefreshCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        DeleteCategoryCommand = new AsyncRelayCommand<CategoryResponse>(DeleteCategoryAsync);
    }

    public void Initialize(User user)
    {
        CurrentUser = user;
        IsAdmin = user?.GetPrimaryRole() == MyShop.Shared.Models.Enums.UserRole.Admin;
    }

    private void UpdateVisibilityStates()
    {
        var hasCategories = Categories?.Count > 0;
        var hasFilteredCategories = FilteredCategories?.Count > 0;

        HasCategories = hasCategories;
        HasFilteredCategories = hasFilteredCategories;
        ShowNoCategoriesState = !hasCategories && !IsLoading;
        ShowNoResultsState = hasCategories && !hasFilteredCategories && !string.IsNullOrWhiteSpace(SearchText);

        System.Diagnostics.Debug.WriteLine($"[UpdateVisibilityStates] HasCategories={HasCategories}, HasFilteredCategories={HasFilteredCategories}, ShowNoCategoriesState={ShowNoCategoriesState}, ShowNoResultsState={ShowNoResultsState}");
    }

    partial void OnSearchTextChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[OnSearchTextChanged] value: '{value}'");
        ApplyFiltersAndSort();
        UpdateVisibilityStates();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[OnSelectedSortOptionChanged] value: '{value}'");
        ApplyFiltersAndSort();
        UpdateVisibilityStates();
    }

    partial void OnCategoriesChanged(ObservableCollection<CategoryResponse> value)
    {
        System.Diagnostics.Debug.WriteLine($"[OnCategoriesChanged] new count: {value?.Count ?? 0}");
    }

    partial void OnFilteredCategoriesChanged(ObservableCollection<CategoryResponse> value)
    {
        System.Diagnostics.Debug.WriteLine($"[OnFilteredCategoriesChanged] new count: {value?.Count ?? 0}");
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[CategoriesViewModel.LoadCategoriesAsync] Starting...");
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var result = await _categoryFacade.LoadCategoriesAsync();

            System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Facade result - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}");

            if (result.IsSuccess && result.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Data count: {result.Data.Count}");
                Categories.Clear();

                foreach (var category in result.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel] Category: {category.Id} - {category.Name} - {category.Description}");
                    // Convert Category model to CategoryResponse for UI
                    var categoryResponse = new CategoryResponse
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        CreatedAt = category.CreatedAt,
                        UpdatedAt = category.UpdatedAt
                    };
                    Categories.Add(categoryResponse);
                }

                System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Categories.Count after add: {Categories.Count}");
                ApplyFiltersAndSort();
                System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] FilteredCategories.Count after filter: {FilteredCategories.Count}");
                UpdateVisibilityStates();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Error: {result.ErrorMessage}");
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Failed to load categories";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[CategoriesViewModel.LoadCategoriesAsync] Stack trace: {ex.StackTrace}");
            HasError = true;
            ErrorMessage = ex.Message;
            LoggingService.Instance.Error("Failed to load categories", ex);
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("[CategoriesViewModel.LoadCategoriesAsync] Finished");
        }
    }

    public void ApplyFiltersAndSort()
    {
        System.Diagnostics.Debug.WriteLine($"[ApplyFiltersAndSort] Starting - Categories.Count: {Categories.Count}");

        var filtered = Categories.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(c =>
                c.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Apply sort
        filtered = SelectedSortOption switch
        {
            "Name A-Z" => filtered.OrderBy(c => c.Name),
            "Name Z-A" => filtered.OrderByDescending(c => c.Name),
            _ => filtered.OrderBy(c => c.Name)
        };

        var filteredList = filtered.ToList();
        System.Diagnostics.Debug.WriteLine($"[ApplyFiltersAndSort] Filtered count: {filteredList.Count}");

        // Create new collection to force binding refresh
        var newFilteredCategories = new ObservableCollection<CategoryResponse>(filteredList);
        FilteredCategories = newFilteredCategories;

        System.Diagnostics.Debug.WriteLine($"[ApplyFiltersAndSort] Finished - FilteredCategories.Count: {FilteredCategories.Count}");
    }

    private async Task DeleteCategoryAsync(CategoryResponse? category)
    {
        if (category == null) return;

        try
        {
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Delete Category",
                $"Delete category '{category.Name}'?\n\nThis may affect product filtering and existing product assignments.");

            if (!confirmResult.IsSuccess || !confirmResult.Data) return;

            IsLoading = true;
            var result = await _categoryFacade.DeleteCategoryAsync(category.Id);

            if (result.IsSuccess)
            {
                Categories.Remove(category);
                ApplyFiltersAndSort();
                _toastService.ShowSuccess("Category deleted successfully");
            }
            else
            {
                _toastService.ShowError(result.ErrorMessage ?? "Failed to delete category");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to delete category", ex);
            _toastService.ShowError("Failed to delete category");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ShowAddCategoryDialogAsync()
    {
        // Will be handled in code-behind for now
        await Task.CompletedTask;
    }

    public async Task ShowEditCategoryDialogAsync(CategoryResponse category)
    {
        // Will be handled in code-behind for now
        await Task.CompletedTask;
    }
}
