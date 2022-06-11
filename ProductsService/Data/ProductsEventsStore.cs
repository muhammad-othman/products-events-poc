using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace ProductsService.Data
{
    public class ProductsEventsStore : IProductsEventsStore
    {
        private readonly IModel rabbitMQChannel;
        private IMongoCollection<ProductEvent> productsEventsCollection;

        private List<Product> productsList = new List<Product>();

        public ProductsEventsStore()
        {
            var dbClient = new MongoClient("mongodb://127.0.0.1:27017");
            var db = dbClient.GetDatabase("productsDB");

            productsEventsCollection = db.GetCollection<ProductEvent>("productsEvents");


            var factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.HostName = "45.63.116.153";
            factory.Port = 5672;

            var connection = factory.CreateConnection();

            rabbitMQChannel = connection.CreateModel();

            rabbitMQChannel.ExchangeDeclare("products.bus", ExchangeType.Fanout, true);

            ReprocessEvents();
        }
        public async Task<Product> AddProducts(Product product)
        {
            product.Id = Guid.NewGuid();
            var productEvent = new ProductEvent
            {
                Id = Guid.NewGuid(),
                Type = EventType.Created,
                ProductData = product,
            };

            await SaveAndProcessProductEvent(productEvent);

            return product;
        }

        public async Task DeleteProducts(Guid id)
        {
            var deletedProduct = productsList.First(x => x.Id == id);
            var productEvent = new ProductEvent
            {
                Id = Guid.NewGuid(),
                Type = EventType.Deleted,
                ProductData = deletedProduct,
            };

            await SaveAndProcessProductEvent(productEvent);
        }

        public async Task<Product> UpdateProducts(Product product)
        {
            var productEvent = new ProductEvent
            {
                Id = Guid.NewGuid(),
                Type = EventType.Updated,
                ProductData = product,
            };

            await SaveAndProcessProductEvent(productEvent);
            return product;
        }

        private async Task SaveAndProcessProductEvent(ProductEvent productEvent)
        {
            await productsEventsCollection.InsertOneAsync(productEvent);

            ProcessProductEvent(productEvent);
            PublishProductEvent(productEvent);
        }

        private void ProcessProductEvent(ProductEvent productEvent)
        {
            switch (productEvent.Type)
            {
                case EventType.Created:
                    productsList.Add(productEvent.ProductData);
                    break;
                case EventType.Updated:
                    var oldProduct = productsList.First(p => p.Id == productEvent.ProductData.Id);
                    oldProduct.Description = productEvent.ProductData.Description;
                    oldProduct.Price = productEvent.ProductData.Price;
                    oldProduct.Name = productEvent.ProductData.Name;
                    break;
                case EventType.Deleted:
                    productsList.RemoveAll(p => p.Id == productEvent.ProductData.Id);
                    break;
            }
        }

        private void PublishProductEvent(ProductEvent productEvent)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productEvent));
            rabbitMQChannel.BasicPublish("products.bus", "", null, body);
        }

        public async Task<List<Product>> GetProducts()
        {
            return productsList;
        }

        public async Task ReprocessEvents(int limit = 0)
        {
            var eventsCollection = productsEventsCollection.Find(_ => true).SortBy(e => e.Created);

            if(limit > 0)
                eventsCollection.Limit(limit);

            var eventsList = await  eventsCollection.ToListAsync();

            productsList = new List<Product>();

            foreach (var @event in eventsList)
            {
                ProcessProductEvent(@event);
            }
        }
    }
}
