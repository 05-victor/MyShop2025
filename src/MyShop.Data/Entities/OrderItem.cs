namespace MyShop.Data.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public int UnitSalePrice { get; set; }
        public int TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}