using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace ProductsService.Data
{
    public class ProductsEventsStore : IProductsEventsStore
    {
        private readonly IModel rabbitMQChannel;
        private IMongoCollection<Product> productsCollection;


        public ProductsEventsStore()
        {
            var dbClient = new MongoClient("mongodb://127.0.0.1:27017");
            var db = dbClient.GetDatabase("productsDB");

            productsCollection = db.GetCollection<Product>("products");


            var factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.HostName = "45.63.116.153";
            factory.Port = 5672;

            var connection = factory.CreateConnection();

            rabbitMQChannel = connection.CreateModel();

            rabbitMQChannel.ExchangeDeclare("products.bus", ExchangeType.Fanout, true);
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

            await ProcessProductEvent(productEvent);

            return product;
        }

        public async Task DeleteProducts(Guid id)
        {
            var deletedProduct = productsCollection.Find(x => x.Id == id).Single();
            var productEvent = new ProductEvent
            {
                Id = Guid.NewGuid(),
                Type = EventType.Deleted,
                ProductData = deletedProduct,
            };

            await ProcessProductEvent(productEvent);
        }

        public async Task<Product> UpdateProducts(Product product)
        {
            var productEvent = new ProductEvent
            {
                Id = Guid.NewGuid(),
                Type = EventType.Updated,
                ProductData = product,
            };

            await ProcessProductEvent(productEvent);
            return product;
        }

        private async Task ProcessProductEvent(ProductEvent productEvent)
        {


            switch (productEvent.Type)
            {
                case EventType.Created:
                    await productsCollection.InsertOneAsync(productEvent.ProductData);
                    break;
                case EventType.Updated:
                    await productsCollection.FindOneAndReplaceAsync(p => p.Id == productEvent.ProductData.Id, productEvent.ProductData);
                    break;
                case EventType.Deleted:
                    await productsCollection.FindOneAndDeleteAsync(p => p.Id == productEvent.ProductData.Id);
                    break;
            }

            PublishProductEvent(productEvent);
        }

        private void PublishProductEvent(ProductEvent productEvent)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productEvent));
            rabbitMQChannel.BasicPublish("products.bus", "", null, body);
        }

        public async Task<List<Product>> GetProducts()
        {
            var products = productsCollection.Find(_ => true);
            return await products.ToListAsync();
        }
    }
}
