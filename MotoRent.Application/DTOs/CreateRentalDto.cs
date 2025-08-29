namespace MotoRent.Application.DTOs
{
    public class CreateRentalDto
    {
        public Guid CourierId { get; set; }
        public Guid MotorcycleId { get; set; }
        public int PlanDays { get; set; }
    }
    public class CompleteRentalDto
    {
        public DateTime ReturnDate { get; set; }
    }
}