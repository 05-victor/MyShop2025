namespace MyShop.Data.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public float UnitSalePrice { get; set; }
        public int TotalPrice { get; set; }
        public Guid ProductId { get; set; }
        public required Product Product { get; set; }
        public Guid OrderId { get; set; }
        public required Order Order { get; set; }
    }
}