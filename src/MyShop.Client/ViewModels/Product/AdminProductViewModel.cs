using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.Client.ViewModels.Product
{
    public partial class AdminProductViewModel : ObservableObject
    {
        // Danh sách product hiển thị trong ListView
        public ObservableCollection<ProductRow> Products { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        public AdminProductViewModel()
        {
        }

        // Tạm thời: load data mẫu. Sau này bạn thay bằng gọi IProductRepository.
        [RelayCommand]
        public void LoadSampleData()
        {
            if (Products.Count > 0) return;

            Products.Clear();

            Products.Add(new ProductRow
            {
                Name = "MacBook Pro 16\"",
                Sku = "MBP16-002",
                Category = "Laptops",
                Price = 2499,
                ImportPrice = 2099,
                Stock = 23,
                Rating = 4.9
            });

            Products.Add(new ProductRow
            {
                Name = "Sony WH-1000XM5",
                Sku = "SONY-008",
                Category = "Audio",
                Price = 399,
                ImportPrice = 299,
                Stock = 41,
                Rating = 4.9
            });

            Products.Add(new ProductRow
            {
                Name = "iPhone 14 Pro Max",
                Sku = "IP14PM-001",
                Category = "Smartphones",
                Price = 1099,
                ImportPrice = 899,
                Stock = 45,
                Rating = 4.8
            });

            Products.Add(new ProductRow
            {
                Name = "Apple Watch Series 9",
                Sku = "AW9-006",
                Category = "Wearables",
                Price = 399,
                ImportPrice = 299,
                Stock = 5,
                Rating = 4.8
            });

            Products.Add(new ProductRow
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

        [RelayCommand]
        private void Refresh()
        {
            Products.Clear();
            LoadSampleData();
        }

        [RelayCommand]
        private void DeleteProduct(ProductRow? row)
        {
            if (row is null) return;
            Products.Remove(row);
        }

        [RelayCommand]
        private void EditProduct(ProductRow? row)
        {
            if (row is null) return;
            // TODO: sau này mở dialog Edit hoặc navigate tới trang Edit
        }

        [RelayCommand]
        private void ViewProduct(ProductRow? row)
        {
            if (row is null) return;
            // TODO: sau này mở dialog / trang chi tiết
        }

        // Dùng khi tạo mới từ dialog
        public void AddProduct(ProductRow newProduct)
        {
            Products.Add(newProduct);
        }
    }

    /// <summary>
    /// Model dùng cho 1 row trong bảng Products.
    /// Mapping từ Product entity ở Shared.
    /// </summary>
    public class ProductRow
    {
        public Guid Id { get; set; }
        public string? Image { get; set; }  // map từ Product.ImageUrl
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }        // SellingPrice
        public decimal ImportPrice { get; set; }  // ImportPrice
        public int Stock { get; set; }            // Quantity
        public double Rating { get; set; }

        public static ProductRow FromProduct(MyShop.Shared.Models.Product product)
        {
            return new ProductRow
            {
                Id = product.Id,
                Image = product.ImageUrl,
                Name = product.Name,
                Sku = product.SKU ?? string.Empty,
                Category = product.CategoryName ?? product.Category ?? string.Empty,
                Price = product.SellingPrice,
                ImportPrice = product.ImportPrice,
                Stock = product.Quantity,
                Rating = product.Rating
            };
        }
    }
}
