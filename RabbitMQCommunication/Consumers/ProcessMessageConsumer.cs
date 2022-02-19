using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQCommunication.InputModel;
using System.Text;
using RabbitMQCommunication.Options;
using RabbitMQCommunication.Services;

namespace RabbitMQCommunication.Consumers
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _provider;

        public ProcessMessageConsumer(IOptions<RabbitMqConfiguration> options, IServiceProvider provider)
        {
            _configuration = options.Value;
            _provider = provider;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host,
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                        queue: _configuration.Queue,
                        durable: false,
                        autoDelete: false,
                        exclusive: false,
                        arguments: null
                        );
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) =>
            {
                var contentArray = args.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<MessageInputModel>(contentString);

                NotifyUser(message);

                _channel.BasicAck(args.DeliveryTag, false);
            };

            _channel.BasicConsume(_configuration.Queue, false, consumer);

            return Task.CompletedTask;
        }

        public void NotifyUser(MessageInputModel message)
        {
            using (var scope = _provider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                notificationService.NotifyUser(message.FromId, message.ToId, message.Content);
            }
        }
    }
}
