namespace MotoRent.Domain.Services
{
    public class RentalCalculationService : IRentalCalculationService
    {
        public decimal CalculateDailyRate(int planDays)
        {
            return planDays switch
            {
                7 => 30.00m,
                15 => 28.00m,
                30 => 22.00m,
                45 => 20.00m,
                50 => 18.00m,
                _ => throw new ArgumentException("Invalid plan days")
            };
        }

        public decimal CalculateTotalAmount(int planDays, DateTime startDate, DateTime endDate)
        {
            var dailyRate = CalculateDailyRate(planDays);
            var rentalDays = (endDate - startDate).Days;
            return dailyRate * rentalDays;
        }

        public decimal CalculateFine(int planDays, DateTime expectedDate, DateTime returnDate)
        {
            if (returnDate >= expectedDate) return 0;

            var earlyDays = (expectedDate - returnDate).Days;
            var dailyRate = CalculateDailyRate(planDays);
            var unusedDaysAmount = dailyRate * earlyDays;

            var finePercentage = planDays switch
            {
                7 => 0.20m,  // 20%
                15 => 0.40m,  // 40%
                _ => 0.00m    // No fine for other plans
            };

            return unusedDaysAmount * finePercentage;
        }

        public decimal CalculateAdditionalAmount(DateTime expectedDate, DateTime returnDate)
        {
            if (returnDate <= expectedDate) return 0;

            var additionalDays = (returnDate - expectedDate).Days;
            return additionalDays * 50.00m; // R$50.00 per additional day
        }
    }
}
