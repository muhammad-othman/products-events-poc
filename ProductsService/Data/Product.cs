namespace ProductsService
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductEvent
    {
        public Guid Id { get; set; }
        public EventType Type { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public Product ProductData { get; set; }
    }

    public enum EventType
    {
        Created,
        Updated,
        Deleted
    }
}