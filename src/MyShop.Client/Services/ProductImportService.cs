using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Facades;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Client.Services;

/// <summary>
/// Service for importing products from Excel/CSV files
/// Supports parsing, validation, and bulk import
/// </summary>
public class ProductImportService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductFacade _productFacade;

    public ProductImportService(
        ICategoryRepository categoryRepository,
        IProductFacade productFacade)
    {
        _categoryRepository = categoryRepository;
        _productFacade = productFacade;
    }
    public class ImportResult
    {
        public bool IsSuccess { get; set; }
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Product> ValidProducts { get; set; } = new();
    }

    /// <summary>
    /// Parse CSV file and return list of products
    /// Expected format: Name,SKU,Description,ImportPrice,SellingPrice,Quantity,Category,Manufacturer,DeviceType
    /// </summary>
    public async Task<ImportResult> ParseCsvAsync(StorageFile file)
    {
        var result = new ImportResult();
        
        try
        {
            var lines = await FileIO.ReadLinesAsync(file);
            
            if (lines.Count < 2) // Need header + at least 1 data row
            {
                result.Errors.Add("File is empty or missing data rows");
                return result;
            }

            var header = lines[0];
            // Expected format: Name,SKU,Description,ImportPrice,SellingPrice,Quantity,Category,Manufacturer,DeviceType,CommissionRate,Status,ImageUrl
            // ImageUrl is optional (column 11), so minimum 11 columns required
            var expectedHeaders = new[] { "Name", "SKU", "Description", "ImportPrice", "SellingPrice", "Quantity", "Category", "Manufacturer", "DeviceType", "CommissionRate", "Status", "ImageUrl (optional)" };
            var minRequiredColumns = 11;
            
            // Validate header
            var headerCols = ParseCsvLine(header);
            if (headerCols.Length < minRequiredColumns)
            {
                result.Errors.Add($"Invalid header format. Expected minimum {minRequiredColumns} columns: Name, SKU, Description, ImportPrice, SellingPrice, Quantity, Category, Manufacturer, DeviceType, CommissionRate, Status, ImageUrl (optional)");
                return result;
            }

            result.TotalRows = lines.Count - 1; // Exclude header

            // Step 1: Collect unique category names from CSV
            var categoryNames = new HashSet<string>();
            for (int i = 1; i < lines.Count; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Length >= expectedHeaders.Length)
                {
                    var categoryName = cols[6].Trim();
                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        categoryNames.Add(categoryName);
                    }
                }
            }

            // Step 2: Load existing categories and create missing ones
            var categoryMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var categoryName in categoryNames)
            {
                try
                {
                    // Try to get existing categories
                    var categoriesResult = await _categoryRepository.GetAllAsync();
                    var existingCategory = categoriesResult.IsSuccess && categoriesResult.Data != null
                        ? categoriesResult.Data.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                        : null;

                    if (existingCategory != null)
                    {
                        categoryMap[categoryName] = existingCategory.Id;
                    }
                    else
                    {
                        // Create new category
                        var newCategory = new Category
                        {
                            Id = Guid.NewGuid(),
                            Name = categoryName,
                            Description = $"Auto-created from CSV import",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        var createResult = await _categoryRepository.CreateAsync(newCategory);
                        if (createResult.IsSuccess && createResult.Data != null)
                        {
                            categoryMap[categoryName] = createResult.Data.Id;
                            System.Diagnostics.Debug.WriteLine($"[ProductImportService] Created category: {categoryName} ({createResult.Data.Id})");
                        }
                        else
                        {
                            result.Errors.Add($"Failed to create category '{categoryName}': {createResult.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing category '{categoryName}': {ex.Message}");
                }
            }

            // Step 3: Parse data rows with mapped category IDs
            for (int i = 1; i < lines.Count; i++)
            {
                try
                {
                    var cols = ParseCsvLine(lines[i]);
                    
                    if (cols.Length < minRequiredColumns)
                    {
                        result.Errors.Add($"Row {i}: Insufficient columns (expected minimum {minRequiredColumns}, got {cols.Length})");
                        result.FailureCount++;
                        continue;
                    }

                    var categoryName = cols[6].Trim();
                    Guid? categoryId = null;
                    
                    if (!string.IsNullOrWhiteSpace(categoryName) && categoryMap.ContainsKey(categoryName))
                    {
                        categoryId = categoryMap[categoryName];
                    }

                    if (!categoryId.HasValue)
                    {
                        result.Errors.Add($"Row {i}: Category '{categoryName}' could not be resolved");
                        result.FailureCount++;
                        continue;
                    }

                    // Parse CommissionRate from column 9 (0-1 decimal or percentage)
                    double commissionRate = 0.1; // Default 10%
                    if (cols.Length > 9 && !string.IsNullOrWhiteSpace(cols[9]))
                    {
                        var commissionInput = cols[9].Trim().Replace("%", "");
                        if (double.TryParse(commissionInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedRate))
                        {
                            // If value > 1, assume it's percentage (e.g., 10 = 10%)
                            commissionRate = parsedRate > 1 ? parsedRate / 100.0 : parsedRate;
                            
                            // Validate range 0-100%
                            if (commissionRate < 0 || commissionRate > 1)
                            {
                                result.Errors.Add($"Row {i}: CommissionRate must be between 0-100%. Using default 10%.");
                                commissionRate = 0.1;
                            }
                        }
                        else
                        {
                            result.Errors.Add($"Row {i}: Invalid CommissionRate '{cols[9]}'. Using default 10%.");
                        }
                    }

                    // Parse Status from column 10
                    var status = "AVAILABLE"; // Default
                    if (cols.Length > 10 && !string.IsNullOrWhiteSpace(cols[10]))
                    {
                        var statusInput = cols[10].Trim().ToUpperInvariant();
                        // Accept: AVAILABLE, OUT_OF_STOCK, DISCONTINUED
                        if (statusInput == "AVAILABLE" || statusInput == "OUT_OF_STOCK" || statusInput == "DISCONTINUED")
                        {
                            status = statusInput;
                        }
                        else
                        {
                            result.Errors.Add($"Row {i}: Invalid Status '{cols[10]}'. Using default 'AVAILABLE'. Valid values: AVAILABLE, OUT_OF_STOCK, DISCONTINUED.");
                        }
                    }

                    // Get ImageUrl from column 11 if available
                    var imageUrlInput = cols.Length > 11 ? cols[11].Trim() : string.Empty;
                    string imageUrl;

                    if (string.IsNullOrWhiteSpace(imageUrlInput))
                    {
                        // No image provided - use placeholder
                        imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";
                    }
                    else if (imageUrlInput.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             imageUrlInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                             imageUrlInput.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Already a URL - use as is
                        imageUrl = imageUrlInput;
                    }
                    else
                    {
                        // Assume it's a local file path - try to upload
                        try
                        {
                            if (File.Exists(imageUrlInput))
                            {
                                System.Diagnostics.Debug.WriteLine($"[ProductImportService] Uploading image from: {imageUrlInput}");
                                var uploadResult = await _productFacade.UploadProductImageForNewProductAsync(imageUrlInput);
                                
                                if (uploadResult.IsSuccess && !string.IsNullOrWhiteSpace(uploadResult.Data))
                                {
                                    imageUrl = uploadResult.Data;
                                    System.Diagnostics.Debug.WriteLine($"[ProductImportService] Image uploaded successfully: {imageUrl}");
                                }
                                else
                                {
                                    result.Errors.Add($"Row {i}: Failed to upload image '{imageUrlInput}' - {uploadResult.ErrorMessage}. Using placeholder.");
                                    imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";
                                }
                            }
                            else
                            {
                                result.Errors.Add($"Row {i}: Image file not found: '{imageUrlInput}'. Using placeholder.");
                                imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Row {i}: Error uploading image '{imageUrlInput}': {ex.Message}. Using placeholder.");
                            imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";
                        }
                    }

                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = cols[0].Trim(),
                        SKU = string.IsNullOrWhiteSpace(cols[1]) ? null : cols[1].Trim(),
                        Description = string.IsNullOrWhiteSpace(cols[2]) ? null : cols[2].Trim(),
                        ImportPrice = ParseDecimal(cols[3], i, "ImportPrice", result),
                        SellingPrice = ParseDecimal(cols[4], i, "SellingPrice", result),
                        Quantity = ParseInt(cols[5], i, "Quantity", result),
                        CategoryName = categoryName,
                        CategoryId = categoryId.Value,
                        Manufacturer = string.IsNullOrWhiteSpace(cols[7]) ? null : cols[7].Trim(),
                        DeviceType = string.IsNullOrWhiteSpace(cols[8]) ? null : cols[8].Trim(),
                        CommissionRate = commissionRate,
                        Status = status,
                        ImageUrl = imageUrl,
                        Rating = 0,
                        RatingCount = 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        result.Errors.Add($"Row {i}: Name is required");
                        result.FailureCount++;
                        continue;
                    }

                    if (product.SellingPrice <= 0)
                    {
                        result.Errors.Add($"Row {i}: SellingPrice must be greater than 0");
                        result.FailureCount++;
                        continue;
                    }

                    if (product.Quantity < 0)
                    {
                        result.Errors.Add($"Row {i}: Quantity cannot be negative");
                        result.FailureCount++;
                        continue;
                    }

                    result.ValidProducts.Add(product);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {i}: {ex.Message}");
                    result.FailureCount++;
                }
            }

            result.IsSuccess = result.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to parse CSV: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Parse CSV line handling quoted fields with commas
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var currentField = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = string.Empty;
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField); // Add last field
        return fields.ToArray();
    }

    private decimal ParseDecimal(string value, int rowIndex, string fieldName, ImportResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        value = value.Trim().Replace(",", ""); // Remove thousand separators
        
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
        {
            return parsed;
        }

        throw new FormatException($"{fieldName} is not a valid number: '{value}'");
    }

    private int ParseInt(string value, int rowIndex, string fieldName, ImportResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        value = value.Trim();
        
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            return parsed;
        }

        throw new FormatException($"{fieldName} is not a valid integer: '{value}'");
    }

    /// <summary>
    /// Generate sample CSV template for download
    /// </summary>
    public async Task<StorageFile> GenerateSampleCsvAsync()
    {
        var savePicker = new Windows.Storage.Pickers.FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("CSV File", new List<string> { ".csv" });
        savePicker.SuggestedFileName = $"product_import_template_{DateTime.Now:yyyyMMdd}";

        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            var sampleData = @"Name,SKU,Description,ImportPrice,SellingPrice,Quantity,Category,Manufacturer,DeviceType,CommissionRate,Status,ImageUrl
iPhone 15 Pro Max,IP15PM,Latest flagship phone with A17 Pro chip,25000000,29990000,50,Smartphones,Apple,Phone,10,AVAILABLE,https://example.com/iphone15.jpg
MacBook Pro 16,MBP16M3,Professional laptop with M3 Max chip,55000000,69990000,20,Laptops,Apple,Laptop,12,AVAILABLE,https://example.com/macbook.jpg
AirPods Pro 2,APP2,Wireless earbuds with active noise cancellation,4500000,5990000,100,Audio,Apple,Headphones,15,AVAILABLE,https://example.com/airpods.jpg
Samsung Galaxy S24,SGS24,Flagship Android phone with AI features,18000000,24990000,75,Smartphones,Samsung,Phone,8,AVAILABLE,
Dell XPS 15,DXPS15,High-performance ultrabook,35000000,44990000,30,Laptops,Dell,Laptop,10,AVAILABLE,";

            await FileIO.WriteTextAsync(file, sampleData);
        }

        return file;
    }
}
