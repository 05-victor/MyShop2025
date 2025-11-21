namespace MyShop.Data.Entities
{
    /// <summary>
    /// Entity representing a product in the MyShop system
    /// </summary>
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

        // Category relationship
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        // Sale Agent (User) relationship
        /// <summary>
        /// ID of the sale agent (user) who published this product
        /// </summary>
        public Guid? SaleAgentId { get; set; }

        /// <summary>
        /// Navigation property to the sale agent (user) who published this product
        /// </summary>
        public User? SaleAgent { get; set; }
    }
}
