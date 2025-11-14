using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Client.Views.Product;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdminProductPage : Page
{
    private readonly ObservableCollection<ProductRow> _products = new();

    public AdminProductPage()
    {
        this.InitializeComponent();

        // Gán ItemsSource cho ListView
        ProductsListView.ItemsSource = _products;

        // Load data mẫu
        LoadSampleProducts();
    }

    #region Sample data

    private void LoadSampleProducts()
    {
        _products.Clear();

        _products.Add(new ProductRow
        {
            Name = "MacBook Pro 16\"",
            Sku = "MBP16-002",
            Category = "Laptops",
            Price = 2499,
            ImportPrice = 2099,
            Stock = 23,
            Rating = 4.9
        });

        _products.Add(new ProductRow
        {
            Name = "Sony WH-1000XM5",
            Sku = "SONY-008",
            Category = "Audio",
            Price = 399,
            ImportPrice = 299,
            Stock = 41,
            Rating = 4.9
        });

        _products.Add(new ProductRow
        {
            Name = "iPhone 14 Pro Max",
            Sku = "IP14PM-001",
            Category = "Smartphones",
            Price = 1099,
            ImportPrice = 899,
            Stock = 45,
            Rating = 4.8
        });

        _products.Add(new ProductRow
        {
            Name = "Apple Watch Series 9",
            Sku = "AW9-006",
            Category = "Wearables",
            Price = 399,
            ImportPrice = 299,
            Stock = 5,
            Rating = 4.8
        });

        _products.Add(new ProductRow
        {
            Name = "AirPods Pro 2",
            Sku = "APP2-003",
            Category = "Audio",
            Price = 249,
            ImportPrice = 179,
            Stock = 8,
            Rating = 4.7
        });
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

        // Bắt buộc set XamlRoot trước khi ShowAsync
        AddProductDialog.XamlRoot = this.XamlRoot;

        await AddProductDialog.ShowAsync();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Products] Export clicked");
        // TODO: sau này nối logic export
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Products] Import clicked");
        // TODO: sau này nối logic import
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Products] Refresh clicked");
        LoadSampleProducts();
    }

    #endregion

    #region Product actions (Edit / Delete / View)

    private void EditProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ProductRow row)
        {
            Debug.WriteLine($"[Products] Edit clicked for {row.Name}");
            // TODO: sau này mở dialog Edit hoặc navigate sang trang Edit
        }
    }

    private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ProductRow row)
        {
            Debug.WriteLine($"[Products] Delete clicked for {row.Name}");
            _products.Remove(row);
        }
    }

    private void ViewProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ProductRow row)
        {
            Debug.WriteLine($"[Products] View clicked for {row.Name}");
            // TODO: sau này mở trang / dialog chi tiết
        }
    }

    #endregion

    #region Pagination

    private void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Products] Prev page clicked");
        // TODO: sau này thêm logic phân trang
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[Products] Next page clicked");
        // TODO: sau này thêm logic phân trang
    }

    #endregion

    #region Add Product Dialog handlers

    private void AddProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NewNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            // Nếu muốn giữ dialog lại khi thiếu tên:
            // args.Cancel = true;
            return;
        }

        // Lấy category từ ComboBox (nếu chọn)
        var categoryItem = NewCategoryComboBox.SelectedItem as ComboBoxItem;
        var category = categoryItem?.Content?.ToString() ?? "Uncategorized";

        int.TryParse(NewStockTextBox.Text, out var stock);
        decimal.TryParse(NewPriceTextBox.Text, out var price);
        decimal.TryParse(NewImportPriceTextBox.Text, out var importPrice);

        var newProduct = new ProductRow
        {
            Name = name,
            Sku = $"SKU-{_products.Count + 1:000}",
            Category = category,
            Stock = stock,
            Price = price,
            ImportPrice = importPrice,
            Rating = 5.0   // tạm cho 5 sao
        };

        _products.Add(newProduct);
    }

    private void AddProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Không cần làm gì, dialog sẽ tự đóng
    }

    #endregion
}

/// <summary>
/// Model đơn giản dùng cho UI ProductsListView.
/// Sau này bạn có thể thay bằng ProductModel hoặc ViewModel riêng.
/// </summary>
public class ProductRow
{
    public string? Image { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal ImportPrice { get; set; }
    public int Stock { get; set; }
    public double Rating { get; set; }
}
