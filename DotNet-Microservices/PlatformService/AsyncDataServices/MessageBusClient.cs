using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient, IDisposable
    {
        private readonly IConfiguration _configuration;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IConnection? _connection;
        private IChannel? _channel;

        private async Task StartChannelAsync()
        {
            if (_connection != null)
            {
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync("trigger", ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        private async Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }

        public async Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
        {
            await StartChannelAsync();

            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection != null && _connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection open, sending message...");

                // To do send the message
                await SendMessageAsync(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Connection is closed, not sending");
            }
        }

        private async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: "trigger",
                routingKey: string.Empty,
                body: body
            );

            Console.WriteLine($"--> We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("MessageBus Disposed");

            if (_channel != null && _channel.IsOpen)
            {
                _channel.CloseAsync();
            }

            if (_connection != null && _connection.IsOpen)
            {
                _connection.CloseAsync();
            }

            _connection = null;
            _channel = null;

            GC.SuppressFinalize(this);
        }
    }
}
