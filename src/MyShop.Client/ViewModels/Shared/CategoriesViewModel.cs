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

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Categories page (used by both Admin and Agent)
/// </summary>
public partial class CategoriesViewModel : ObservableObject
{
    private readonly CategoriesFacade _facade;
    private readonly MyShop.Core.Interfaces.Services.IDialogService _dialogService;
    private readonly MyShop.Core.Interfaces.Services.IToastService _toastService;

    [ObservableProperty]
    private User? _currentUser;

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

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand DeleteCategoryCommand { get; }

    public CategoriesViewModel(CategoriesFacade facade, MyShop.Core.Interfaces.Services.IDialogService dialogService, MyShop.Core.Interfaces.Services.IToastService toastService)
    {
        _facade = facade;
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
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFiltersAndSort();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        ApplyFiltersAndSort();
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var result = await _facade.GetAllCategoriesAsync();

            if (result.IsSuccess && result.Data != null)
            {
                Categories.Clear();
                foreach (var category in result.Data)
                {
                    Categories.Add(category);
                }

                ApplyFiltersAndSort();
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Failed to load categories";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            LoggingService.Instance.Error("Failed to load categories", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ApplyFiltersAndSort()
    {
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

        FilteredCategories.Clear();
        foreach (var category in filtered)
        {
            FilteredCategories.Add(category);
        }
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
            var result = await _facade.DeleteCategoryAsync(category.Id);

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
