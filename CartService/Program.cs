using CartService;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

class Program
{
    static void Main(string[] args)
    {

        var factory = new ConnectionFactory();

        factory.UserName = "guest";
        factory.Password = "guest";

        factory.HostName = "45.63.116.153";
        factory.Port = 5672;


        var connection = factory.CreateConnection();

        var rabbitMQChannel = connection.CreateModel();

        var queueName = "products.subscriber.cart";

        rabbitMQChannel.QueueDeclare(queueName, true, false, false);
        rabbitMQChannel.QueueBind(queueName, "products.bus", "");

        var consumer = new EventingBasicConsumer(rabbitMQChannel);


        consumer.Received += ProductEventReceived;

        rabbitMQChannel.BasicConsume(queueName, true, consumer);

        Console.ReadLine();
    }

    private static void ProductEventReceived(object? sender, BasicDeliverEventArgs e)
    {
        var body = e.Body.ToArray();
        var jsonString = Encoding.UTF8.GetString(body);
        var productEvent = JsonConvert.DeserializeObject<ProductEvent>(jsonString);

        Console.WriteLine(productEvent);
        Console.WriteLine("");
        Console.WriteLine("=================================");
        Console.WriteLine("=================================");
        Console.WriteLine("=================================");
    }
}
