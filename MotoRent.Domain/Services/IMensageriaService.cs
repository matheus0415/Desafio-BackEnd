namespace MotoRent.Domain.Services
{
    public interface IMessagingService
    {
        Task PublishMotorcycleRegisteredAsync(Guid motorcycleId, string licensePlate, int year);
        Task PublishYear2024NotificationAsync(Guid motorcycleId, string licensePlate, int year);
    }
}
