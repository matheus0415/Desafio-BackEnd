namespace MotoRent.Domain.Services
{
    public interface IRentalCalculationService
    {
        decimal CalculateDailyRate(int planDays);
        decimal CalculateTotalAmount(int planDays, DateTime startDate, DateTime endDate);
        decimal CalculateFine(int planDays, DateTime expectedDate, DateTime returnDate);
        decimal CalculateAdditionalAmount(DateTime expectedDate, DateTime returnDate);
    }
}
