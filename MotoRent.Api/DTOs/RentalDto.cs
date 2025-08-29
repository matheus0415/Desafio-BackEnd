namespace MotoRent.Api.DTOs
{
    public class RentalDto
    {
        public Guid Id { get; set; }
        public Guid CourierId { get; set; }
        public Guid MotorcycleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int PlanDays { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? FineAmount { get; set; }
        public decimal? AdditionalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        
        // Related data
        public string? CourierName { get; set; }
        public string? MotorcycleModel { get; set; }
        public string? MotorcycleLicensePlate { get; set; }
    }

    public class CreateRentalDto
    {
    public Guid CourierId { get; set; }
        public Guid MotorcycleId { get; set; }
        public int PlanDays { get; set; } // 7, 15, 30, 45, 50
    }

    public class CompleteRentalDto
    {
        public DateTime ReturnDate { get; set; }
    }

    public class RentalResponseDto
    {
        public decimal TotalAmount { get; set; }
        public decimal? FineAmount { get; set; }
        public decimal? AdditionalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
