namespace MyShop.Data.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = "PENDING";

        public string PaymentStatus { get; set; } = "UNPAID";

        public int TotalAmount { get; set; }

        public int DiscountAmount { get; set; }

        public int ShippingFee { get; set; }

        public int TaxAmount { get; set; }

        public int GrandTotal { get; set; }

        public string? Note { get; set; }

        public Guid CustomerId { get; set; }

        public User Customer { get; set; } = null!;

        public Guid SaleAgentId { get; set; }

        public User SaleAgent { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property to order items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}