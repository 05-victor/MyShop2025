namespace MyShop.Data.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public float UnitSalePrice { get; set; }
        public int TotalPrice { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; }
        public int OrderId { get; set; }
        public required Order Order { get; set; }
    }
}