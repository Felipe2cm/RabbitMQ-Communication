using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQCommunication.InputModel;
using RabbitMQCommunication.Options;
using System.Text;

namespace RabbitMQCommunication.Controllers
{
    [ApiController]
    public class MessageController: Controller
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;
        private readonly string QUEUE_NAME;        

        public MessageController(IOptions<RabbitMqConfiguration> options)
        {
            _rabbitMqConfiguration = options.Value;


            _factory = new ConnectionFactory
            { 
                HostName = _rabbitMqConfiguration.Host,
            };

            QUEUE_NAME = _rabbitMqConfiguration.Host;

        }

        [HttpPost("PostMessage")]
        public IActionResult PostMessage(MessageInputModel message)
        {
            using (var connection = _factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: QUEUE_NAME,
                        durable: false,
                        autoDelete: false,
                        exclusive: false,
                        arguments: null
                        );

                    var stringfieldMessage = JsonConvert.SerializeObject(message);
                    var bytesMessage = Encoding.UTF8.GetBytes(stringfieldMessage);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: QUEUE_NAME,
                        basicProperties: null,
                        body: bytesMessage);                    
                }
            }

            return Accepted();
        }


    }
}
