using Microsoft.Extensions.Options;
using MotoRentalService.CrossCutting.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MotoRentalService.Infrastructure.Messaging
{
    public abstract class RabbitMQConsumer
    {
        private readonly ILoggerManager _logger;
        private readonly RabbitMQConfig _rabbitMQConfig;
        private IConnection? _connection;
        private IModel? _channel;
        private EventingBasicConsumer? _consumer;
        private CancellationTokenSource? _cancellationTokenSource;

        protected RabbitMQConsumer(IOptions<RabbitMQConfig> rabbitMQConfig, ILoggerManager loggerManager)
        {
            _logger = loggerManager;
            _rabbitMQConfig = rabbitMQConfig.Value;
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var factory = new ConnectionFactory() { HostName = _rabbitMQConfig.Hostname };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _rabbitMQConfig.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await ProcessMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _rabbitMQConfig.QueueName, autoAck: true, consumer: _consumer);

            _logger.LogInfo($"RabbitMQ: Consumer initialized successfully in Hostname {_rabbitMQConfig.Hostname} and QueueName {_rabbitMQConfig.QueueName}.");

            // Keep the application running until cancellation is requested
            await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
        }

        protected abstract Task ProcessMessageAsync(string message);

        public async Task StopConsumingAsync()
        {
            if (_consumer != null)
            {
                if (_consumer.ConsumerTags.Any())
                {
                    _channel?.BasicCancel(_consumer.ConsumerTags.First());
                }
                _consumer = null;
            }

            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            await Task.CompletedTask;
        }
    }
}