using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentProductsViewModel : ObservableObject
{
    private readonly IProductRepository _productRepository;
    private List<MyShop.Shared.Models.Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> _products;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All Categories";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    public SalesAgentProductsViewModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
        Products = new ObservableCollection<ProductViewModel>();
    }

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var result = await _productRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                Products = new ObservableCollection<ProductViewModel>();
                return;
            }
            
            Products = new ObservableCollection<ProductViewModel>(
                result.Data.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.CategoryName ?? p.Category ?? "Uncategorized",
                    Price = p.SellingPrice,
                    CommissionRate = (int)(p.CommissionRate * 100),
                    Stock = p.Quantity,
                    ImageUrl = p.ImageUrl ?? "/Assets/placeholder-product.png"
                }).ToList()
            );
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Error loading products: {ex.Message}");
            Products = new ObservableCollection<ProductViewModel>();
        }
    }

    [RelayCommand]
    private void Search(string query)
    {
        SearchQuery = query;
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Products.Clear();
        foreach (var product in filtered)
        {
            Products.Add(new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.CategoryName ?? product.Category ?? "Uncategorized",
                Price = product.SellingPrice,
                CommissionRate = (int)(product.CommissionRate * 100),
                Stock = product.Quantity,
                ImageUrl = product.ImageUrl ?? "/Assets/placeholder-product.png"
            });
        }
    }

    [RelayCommand]
    private void FilterCategory(string category)
    {
        SelectedCategory = category;
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All Categories")
        {
            filtered = filtered.Where(p => p.Category == SelectedCategory);
        }

        Products.Clear();
        foreach (var product in filtered)
        {
            Products.Add(new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.CategoryName ?? product.Category ?? "Uncategorized",
                Price = product.SellingPrice,
                CommissionRate = (int)(product.CommissionRate * 100),
                Stock = product.Quantity,
                ImageUrl = product.ImageUrl ?? "/Assets/placeholder-product.png"
            });
        }
    }

    [RelayCommand]
    private void ViewProductDetails(Guid productId)
    {
        // TODO: Navigate to product details page when ProductDetailsPage is created
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] View product: {productId}");
    }
}

public partial class ProductViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private int _commissionRate;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    public decimal CommissionAmount => Price * CommissionRate / 100;
}
