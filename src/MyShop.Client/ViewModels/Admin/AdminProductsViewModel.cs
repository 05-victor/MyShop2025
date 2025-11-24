using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminProductsViewModel : ObservableObject
{
        private readonly IProductRepository _productRepository;

        // Danh sách product hiển thị trong ListView
        public ObservableCollection<ProductRow> Products { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        public AdminProductsViewModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Load products from repository
        /// </summary>
        [RelayCommand]
        public async Task LoadProductsAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                AppLogger.Info("Loading products from repository...");

                var products = await _productRepository.GetAllAsync();

                Products.Clear();

                foreach (var product in products)
                {
                    Products.Add(ProductRow.FromProduct(product));
                }

                AppLogger.Success($"Loaded {Products.Count} products successfully");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load products: {ex.Message}";
                AppLogger.Error("Failed to load products", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadProductsAsync();
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
            // TODO: Open EditProductDialog when created
            System.Diagnostics.Debug.WriteLine($"[AdminProductViewModel] Edit product: {row.Name}");
        }

        [RelayCommand]
        private void ViewProduct(ProductRow? row)
        {
            if (row is null) return;
            // TODO: Open ProductDetailsDialog when created
            System.Diagnostics.Debug.WriteLine($"[AdminProductViewModel] View product: {row.Name}");
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