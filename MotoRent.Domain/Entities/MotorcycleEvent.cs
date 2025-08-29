namespace MotoRent.Domain.Entities
{
    public class MotorcycleEvent
    {
        public Guid Id { get; set; }
        public Guid MotorcycleId { get; set; }
        public string EventType { get; set; } = "MotorcycleRegistered";
        public DateTime EventDate { get; set; } = DateTime.UtcNow;
        public string? AdditionalData { get; set; }
        public bool Processed { get; set; } = false;
        
        // Relationships
        public virtual Motorcycle Motorcycle { get; set; } = null!;
    }
}
