using MotoRent.Domain.Entities;
using MotoRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MotoRent.Infrastructure.Services
{
    public class MessageConsumerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private const string EXCHANGE_NAME = "motorrent_exchange";
        private const string QUEUE_YEAR_2024 = "year_2024";

        public MessageConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange and queue
            _channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(QUEUE_YEAR_2024, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QUEUE_YEAR_2024, EXCHANGE_NAME, "motorcycle.year.2024");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    await ProcessMessageAsync(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QUEUE_YEAR_2024,
                                autoAck: false,
                                consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(string message)
        {
            var eventData = JsonSerializer.Deserialize<JsonElement>(message);
            
            if (eventData.TryGetProperty("EventType", out var typeProp) && typeProp.GetString() == "Year2024Notification")
            {
                var motorcycleId = Guid.Parse(eventData.GetProperty("MotorcycleId").GetString()!);
                var licensePlate = eventData.GetProperty("LicensePlate").GetString()!;
                var year = eventData.GetProperty("Year").GetInt32();

                await SaveYear2024EventAsync(motorcycleId, licensePlate, year);
            }
        }

        private async Task SaveYear2024EventAsync(Guid motorcycleId, string licensePlate, int year)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MotoRentDbContext>();

            var eventData = new MotorcycleEvent
            {
                Id = Guid.NewGuid(),
                MotorcycleId = motorcycleId,
                EventType = "Year2024Notification",
                EventDate = DateTime.UtcNow,
                AdditionalData = $"Motorcycle {licensePlate} from year {year} - Special notification",
                Processed = true
            };

            context.MotorcycleEvents.Add(eventData);
            await context.SaveChangesAsync();
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
