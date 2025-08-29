namespace MotoRent.Domain.Entities
{
    public class Rental
    {
        public Guid Id { get; set; }
        public Guid CourierId { get; set; }
        public Guid MotorcycleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int PlanDays { get; set; } // 7, 15, 30, 45, 50
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? FineAmount { get; set; }
        public decimal? AdditionalAmount { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        // Relationships
        public virtual Courier Courier { get; set; } = null!;
        public virtual Motorcycle Motorcycle { get; set; } = null!;
    }
}
