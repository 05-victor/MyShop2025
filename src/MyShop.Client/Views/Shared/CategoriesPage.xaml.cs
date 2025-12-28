using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.ViewModels.Shared;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Client.Views.Shared;

public sealed partial class CategoriesPage : Page
{
    public CategoriesViewModel ViewModel { get; }
    private readonly MyShop.Core.Interfaces.Services.IDialogService _dialogService;
    private readonly MyShop.Core.Interfaces.Services.IToastService _toastService;

    public ICommand AddCategoryCommand { get; }

    public CategoriesPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CategoriesViewModel>();
        _dialogService = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Services.IDialogService>();
        _toastService = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Services.IToastService>();
        DataContext = ViewModel;

        AddCategoryCommand = new RelayCommand(AddCategory);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MyShop.Shared.Models.User user)
        {
            ViewModel.Initialize(user);
            if (ViewModel.LoadCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand loadCmd)
                await loadCmd.ExecuteAsync(null);
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer _, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        if (ViewModel.RefreshCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand refreshCmd)
            await refreshCmd.ExecuteAsync(null);
    }

    private void SearchBox_EscapePressed(KeyboardAccelerator _, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.ClearSearchCommand.Execute(null);
        args.Handled = true;
    }

    private Visibility ShowNoResultsState(int totalCount, int filteredCount, string searchText)
    {
        return totalCount > 0 && filteredCount == 0 && !string.IsNullOrWhiteSpace(searchText)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void AddCategory()
    {
        AddCategoryButton_Click(null, null);
    }

    private async void AddCategoryButton_Click(object? _, RoutedEventArgs? __)
    {
        var nameBox = new TextBox
        {
            PlaceholderText = "e.g. Laptops",
            MaxLength = 100
        };

        var descriptionBox = new TextBox
        {
            PlaceholderText = "e.g. Laptops and notebooks, gaming laptops, ultrabooks...",
            MaxLength = 500,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 80
        };

        var dialog = new ContentDialog
        {
            Title = "Add Category",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            RequestedTheme = ActualTheme
        };

        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock { Text = "Name *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        content.Children.Add(nameBox);
        content.Children.Add(new TextBlock { Text = "Description", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
        content.Children.Add(descriptionBox);

        dialog.Content = content;

        // Disable Save button if name is empty
        nameBox.TextChanged += (s, args) =>
        {
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(nameBox.Text);
        };
        dialog.IsPrimaryButtonEnabled = false;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var request = new CreateCategoryRequest
            {
                Name = nameBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(descriptionBox.Text) ? null : descriptionBox.Text.Trim()
            };

            await CreateCategoryAsync(request);
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs _)
    {
        if (sender is not Button button || button.Tag is not CategoryResponse category)
            return;

        var nameBox = new TextBox
        {
            Text = category.Name,
            MaxLength = 100
        };

        var descriptionBox = new TextBox
        {
            Text = category.Description ?? string.Empty,
            MaxLength = 500,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            Height = 80
        };

        var dialog = new ContentDialog
        {
            Title = "Edit Category",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
            RequestedTheme = ActualTheme
        };

        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock { Text = "Name *", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        content.Children.Add(nameBox);
        content.Children.Add(new TextBlock { Text = "Description", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
        content.Children.Add(descriptionBox);

        dialog.Content = content;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var request = new UpdateCategoryRequest
            {
                Name = nameBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(descriptionBox.Text) ? null : descriptionBox.Text.Trim()
            };

            await UpdateCategoryAsync(category.Id, request);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs _)
    {
        if (sender is not Button button || button.Tag is not CategoryResponse category)
            return;

        if (ViewModel.DeleteCategoryCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand<CategoryResponse> deleteCmd)
            await deleteCmd.ExecuteAsync(category);
    }

    private async System.Threading.Tasks.Task CreateCategoryAsync(CreateCategoryRequest request)
    {
        try
        {
            ViewModel.IsLoading = true;
            var facade = App.Current.Services.GetRequiredService<Facades.CategoriesFacade>();
            var result = await facade.CreateCategoryAsync(request);

            if (result.IsSuccess && result.Data != null)
            {
                ViewModel.Categories.Add(result.Data);
                ViewModel.FilteredCategories.Add(result.Data);
                _toastService.ShowSuccess("Category created successfully");
            }
            else
            {
                _toastService.ShowError(result.ErrorMessage ?? "Failed to create category");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to create category", ex);
            _toastService.ShowError("Failed to create category");
        }
        finally
        {
            ViewModel.IsLoading = false;
        }
    }

    private async System.Threading.Tasks.Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        try
        {
            ViewModel.IsLoading = true;
            var facade = App.Current.Services.GetRequiredService<Facades.CategoriesFacade>();
            var result = await facade.UpdateCategoryAsync(id, request);

            if (result.IsSuccess && result.Data != null)
            {
                var existing = ViewModel.Categories.FirstOrDefault(c => c.Id == id);
                if (existing != null)
                {
                    var index = ViewModel.Categories.IndexOf(existing);
                    ViewModel.Categories[index] = result.Data;
                }

                // Re-apply filters
                ViewModel.ApplyFiltersAndSort();
                _toastService.ShowSuccess("Category updated successfully");
            }
            else
            {
                _toastService.ShowError(result.ErrorMessage ?? "Failed to update category");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to update category", ex);
            _toastService.ShowError("Failed to update category");
        }
        finally
        {
            ViewModel.IsLoading = false;
        }
    }
}
