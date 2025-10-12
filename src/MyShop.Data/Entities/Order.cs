namespace MyShop.Data.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int FinalPrice { get; set; }
    }
}