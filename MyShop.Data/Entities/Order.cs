namespace MyShop.Data.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int FinalPrice { get; set; }
    }
}