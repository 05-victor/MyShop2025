

namespace MyShop.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string SKU { get; set; }
        public required string Name { get; set; }
        public string? Manufacturer { get; set; }
        public string? DeviceType { get; set; }
        public int ImportPrice { get; set; }
        public int SellingPrice { get; set; }
        public int Quantity { get; set; }
        public double CommissionRate { get; set; }
        public string? Status { get; set; } = "AVAILABLE";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CategoryId { get; set; }
        public required Category Category { get; set; }
    }
}
