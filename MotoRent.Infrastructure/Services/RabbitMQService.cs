using MotoRent.Domain.Services;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MotoRent.Infrastructure.Services
{
    public class RabbitMQService : IMessagingService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string EXCHANGE_NAME = "motorrent_exchange";
        private const string QUEUE_MOTORCYCLE_REGISTERED = "motorcycle_registered";
        private const string QUEUE_YEAR_2024 = "year_2024";

        public RabbitMQService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, durable: true);

            // Declare queues
            _channel.QueueDeclare(QUEUE_MOTORCYCLE_REGISTERED, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(QUEUE_YEAR_2024, durable: true, exclusive: false, autoDelete: false);

            // Bind queues to exchange
            _channel.QueueBind(QUEUE_MOTORCYCLE_REGISTERED, EXCHANGE_NAME, "motorcycle.registered");
            _channel.QueueBind(QUEUE_YEAR_2024, EXCHANGE_NAME, "motorcycle.year.2024");
        }

        public async Task PublishMotorcycleRegisteredAsync(Guid motorcycleId, string licensePlate, int year)
        {
            var message = new
            {
                MotorcycleId = motorcycleId,
                LicensePlate = licensePlate,
                Year = year,
                EventDate = DateTime.UtcNow,
                EventType = "MotorcycleRegistered"
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: EXCHANGE_NAME,
                routingKey: "motorcycle.registered",
                basicProperties: null,
                body: body);

            await Task.CompletedTask;
        }

        public async Task PublishYear2024NotificationAsync(Guid motorcycleId, string licensePlate, int year)
        {
            if (year == 2024)
            {
                var message = new
                {
                    MotorcycleId = motorcycleId,
                    LicensePlate = licensePlate,
                    Year = year,
                    EventDate = DateTime.UtcNow,
                    EventType = "Year2024Notification"
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: "motorcycle.year.2024",
                    basicProperties: null,
                    body: body);
            }

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
