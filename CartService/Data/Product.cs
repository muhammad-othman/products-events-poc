
namespace CartService.Data
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

    }

    public class ProductEvent
    {
        public Guid Id { get; set; }
        public EventType Type { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public Product ProductData { get; set; }


        public override string ToString()
        {
            return $"Event Type: {Enum.GetName(typeof(EventType), Type)} \nProduct Name: {ProductData.Name} \nProductPrice: {ProductData.Price}";
        }
    }

    public enum EventType
    {
        Created,
        Updated,
        Deleted
    }
}
