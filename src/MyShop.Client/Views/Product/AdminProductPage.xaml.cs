using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Product;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Client.Views.Product;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdminProductPage : Page
{
    public AdminProductViewModel ViewModel { get; }

    public AdminProductPage()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<AdminProductViewModel>();
        this.DataContext = ViewModel;

        Loaded += AdminProductPage_Loaded;
    }

    #region Sample data

    private void AdminProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Tạm thời dùng data mẫu
        ViewModel.LoadSampleDataCommand.Execute(null);
    }

    #endregion

    #region Button handlers (Header)

    private async void AddProductButton_Click(object sender, RoutedEventArgs e)
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

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: sau này thêm logic export
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: sau này thêm logic import
    }

    private void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: sau này thêm phân trang
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: sau này thêm phân trang
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

        var newProduct = new ProductRow
        {
            Name = name,
            Sku = $"SKU-{ViewModel.Products.Count + 1:000}",
            Category = category,
            Stock = stock,
            Price = price,
            ImportPrice = importPrice,
            Rating = 5.0 // tạm thời
        };

        ViewModel.AddProduct(newProduct);
    }

    private void AddProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Không cần làm gì, dialog tự đóng
    }

    #endregion
}
