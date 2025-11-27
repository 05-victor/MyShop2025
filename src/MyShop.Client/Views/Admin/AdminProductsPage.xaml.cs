using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Client.Views.Admin;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdminProductsPage : Page
{
    public AdminProductsViewModel ViewModel { get; }

    public AdminProductsPage()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<AdminProductsViewModel>();
        this.DataContext = ViewModel;

        Loaded += AdminProductPage_Loaded;
    }

    #region Sample data

    private async void AdminProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Load products from repository
        if (ViewModel.LoadProductsCommand.CanExecute(null))
        {
            await ViewModel.LoadProductsCommand.ExecuteAsync(null);
        }
    }

    #endregion

    #region Button handlers (Header)

    private async void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset field trong dialog
            NewNameTextBox.Text = string.Empty;
            NewStockTextBox.Text = string.Empty;
            NewPriceTextBox.Text = string.Empty;
            NewImportPriceTextBox.Text = string.Empty;
            NewDescriptionTextBox.Text = string.Empty;
            NewCategoryComboBox.SelectedIndex = -1;

            AddProductDialog.XamlRoot = this.XamlRoot;
            await AddProductDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] AddProductButton_Click failed: {ex.Message}");
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement export to CSV/Excel when FileSavePicker is integrated
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Export products requested");
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement import from CSV/Excel when FileOpenPicker is integrated
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Import products requested");
    }

    private void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement pagination when ViewModel has PreviousPage method
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Previous page requested");
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement pagination when ViewModel has NextPage method
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Next page requested");
    }

    #endregion

    #region Add Product Dialog handlers

    private void AddProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NewNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            // Nếu muốn chặn đóng dialog khi thiếu tên:
            // args.Cancel = true;
            return;
        }

        var categoryItem = NewCategoryComboBox.SelectedItem as ComboBoxItem;
        var category = categoryItem?.Content?.ToString() ?? "Uncategorized";

        int.TryParse(NewStockTextBox.Text, out var stock);
        decimal.TryParse(NewPriceTextBox.Text, out var price);
        decimal.TryParse(NewImportPriceTextBox.Text, out var importPrice);

        // TODO: Implement AddProduct through ViewModel
        // For now, just close the dialog
        System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] Add product requested: {name}");
        // Would call: await ViewModel.AddProductCommand.ExecuteAsync(productData);
    }

    private void AddProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Không cần làm gì, dialog tự đóng
    }

    #endregion

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }
}
