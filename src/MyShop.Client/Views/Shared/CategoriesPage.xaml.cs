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

        System.Diagnostics.Debug.WriteLine("[CategoriesPage.ctor] Setting DataContext");
        DataContext = ViewModel;
        System.Diagnostics.Debug.WriteLine($"[CategoriesPage.ctor] DataContext set - ViewModel is {(DataContext == ViewModel ? "correct" : "WRONG")}");

        AddCategoryCommand = new RelayCommand(AddCategory);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        System.Diagnostics.Debug.WriteLine("[CategoriesPage.OnNavigatedTo] Called");

        if (e.Parameter is MyShop.Shared.Models.User user)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoriesPage.OnNavigatedTo] User: {user.Username}");
            ViewModel.Initialize(user);
            System.Diagnostics.Debug.WriteLine($"[CategoriesPage.OnNavigatedTo] ViewModel initialized - IsAdmin: {ViewModel.IsAdmin}");

            if (ViewModel.LoadCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand loadCmd)
            {
                System.Diagnostics.Debug.WriteLine("[CategoriesPage.OnNavigatedTo] Executing LoadCommand");
                await loadCmd.ExecuteAsync(null);
                System.Diagnostics.Debug.WriteLine($"[CategoriesPage.OnNavigatedTo] LoadCommand completed - FilteredCategories.Count: {ViewModel.FilteredCategories.Count}");

                // Force UI refresh using DispatcherQueue
                System.Diagnostics.Debug.WriteLine("[CategoriesPage.OnNavigatedTo] Queuing UI refresh");
                DispatcherQueue.TryEnqueue(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"[CategoriesPage.OnNavigatedTo] UI refresh - FilteredCategories.Count: {ViewModel.FilteredCategories.Count}");
                });
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[CategoriesPage.OnNavigatedTo] No user parameter passed!");
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
            var categoryFacade = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Facades.ICategoryFacade>();
            var result = await categoryFacade.CreateCategoryAsync(request.Name, request.Description ?? string.Empty);

            if (result.IsSuccess && result.Data != null)
            {
                // Reload all categories to get fresh data
                if (ViewModel.LoadCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand loadCmd)
                    await loadCmd.ExecuteAsync(null);

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
            var categoryFacade = App.Current.Services.GetRequiredService<MyShop.Core.Interfaces.Facades.ICategoryFacade>();
            var result = await categoryFacade.UpdateCategoryAsync(id, request.Name ?? string.Empty, request.Description ?? string.Empty);

            if (result.IsSuccess && result.Data != null)
            {
                // Reload all categories to get fresh data
                if (ViewModel.LoadCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand loadCmd)
                    await loadCmd.ExecuteAsync(null);

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
