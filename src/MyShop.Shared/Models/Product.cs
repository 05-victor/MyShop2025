namespace MyShop.Shared.Models;

/// <summary>
/// Product entity model
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int Quantity { get; set; }
    
    // Category references
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Category { get; set; }  // For backward compatibility
    
    public string? Manufacturer { get; set; }
    public string? DeviceType { get; set; }
    public double CommissionRate { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public string Status { get; set; } = "AVAILABLE";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
