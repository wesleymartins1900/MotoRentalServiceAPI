using Microsoft.Extensions.Options;
using MotoRentalService.CrossCutting.Messaging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace MotoRentalService.Infrastructure.Messaging
{
    public class RabbitMqProducer : IMessageProducer
    {
        private readonly RabbitMQConfig _rabbitMQConfig;
        private IConnection _connection;

        public RabbitMqProducer(IOptions<RabbitMQConfig> rabbitMQConfig)
        {
            _rabbitMQConfig = rabbitMQConfig.Value;
            CreateConnection();
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.QueueDeclare(queue: _rabbitMQConfig.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                await Task.Run(() =>
                {
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: _rabbitMQConfig.QueueName,
                        basicProperties: null,
                        body: body);
                });
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _rabbitMQConfig.Hostname };
                _connection = factory.CreateConnection();
                Console.WriteLine($"RabbitMq: Connection created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMq: An error occurred while starting the connection.\r\nException Details: {ex}.");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
                return true;

            CreateConnection();

            return _connection != null;
        }
    }
}
