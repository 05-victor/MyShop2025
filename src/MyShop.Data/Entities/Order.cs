namespace MyShop.Data.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int FinalPrice { get; set; }
    }
}