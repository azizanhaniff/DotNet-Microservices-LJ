
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
        }

        private IConnection? _connection;
        private IChannel? _channel;
        private string _queueName;

        private async Task InitializeRabbitMQ()
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

                var _queue = await _channel.QueueDeclareAsync();
                _queueName = _queue.QueueName;

                await _channel.QueueBindAsync(
                    queue: _queueName,
                    exchange: "trigger",
                    routingKey: string.Empty
                );

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

                Console.WriteLine("--> Listening on the Message Bus");
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMQ();

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Event Received!");

                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventProcessor.ProcessEvent(notificationMessage);
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: true,
                consumer: consumer
            );
        }

        public override void Dispose()
        {
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
