using MotoRent.Application.DTOs;
using MotoRent.Domain.Entities;
using MotoRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MotoRent.Domain.Services;

namespace MotoRent.Application.Services
{
    public class RentalService
    {
        private readonly MotoRentDbContext _context;
        private readonly IRentalCalculationService _calculationService;

        public RentalService(MotoRentDbContext context, IRentalCalculationService calculationService)
        {
            _context = context;
            _calculationService = calculationService;
        }

        public async Task<Rental?> GetByIdAsync(Guid id)
        {
            return await _context.Rentals
                .Include(l => l.Courier)
                .Include(l => l.Motorcycle)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(CreateRentalDto dto)
        {
            if (!IsValidPlan(dto.PlanDays))
                return (false, "Invalid plan days. Use 7, 15, 30, 45 or 50 days.");

            Motorcycle? motorcycle = await _context.Motorcycles.FindAsync(dto.MotorcycleId);
            if (motorcycle == null)
                return (false, "Motorcycle not found.");
            if (!motorcycle.Available)
                return (false, "Motorcycle is not available for rental.");

            Courier? courier = await _context.Couriers.FindAsync(dto.CourierId);
            if (courier == null)
                return (false, "Courier not found.");
            if (!courier.Enabled)
                return (false, "Courier is not enabled.");
            if (!courier.LicenseType.Contains("A"))
                return (false, "Courier must have category A license to rent motorcycles.");

            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = startDate.AddDays(dto.PlanDays);
            DateTime expectedEndDate = endDate;
            decimal dailyRate = _calculationService.CalculateDailyRate(dto.PlanDays);
            decimal totalAmount = _calculationService.CalculateTotalAmount(dto.PlanDays, startDate, endDate);

            Rental rental = new Rental
            {
                Id = Guid.NewGuid(),
                CourierId = dto.CourierId,
                MotorcycleId = dto.MotorcycleId,
                StartDate = startDate,
                EndDate = endDate,
                ExpectedEndDate = expectedEndDate,
                PlanDays = dto.PlanDays,
                DailyRate = dailyRate,
                TotalAmount = totalAmount,
                Status = "Active",
                RegistrationDate = DateTime.UtcNow
            };
            _context.Rentals.Add(rental);
            motorcycle.Available = false;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> CompleteAsync(Guid id, DateTime returnDate)
        {
            Rental? rental = await _context.Rentals.Include(l => l.Motorcycle).FirstOrDefaultAsync(l => l.Id == id);
            if (rental == null) return (false, "Rental not found.");
            if (rental.Status != "Active")
                return (false, "Rental is not active.");
            if (returnDate < rental.StartDate)
                return (false, "Return date cannot be before start date.");

            decimal fineAmount = _calculationService.CalculateFine(rental.PlanDays, rental.ExpectedEndDate, returnDate);
            decimal additionalAmount = _calculationService.CalculateAdditionalAmount(rental.ExpectedEndDate, returnDate);
            int effectiveDays = (returnDate - rental.StartDate).Days;
            decimal totalDaysAmount = rental.DailyRate * effectiveDays;
            decimal finalTotalAmount = totalDaysAmount + fineAmount + additionalAmount;

            rental.ReturnDate = returnDate;
            rental.FineAmount = fineAmount;
            rental.AdditionalAmount = additionalAmount;
            rental.TotalAmount = finalTotalAmount;
            rental.Status = "Completed";
            rental.Motorcycle.Available = true;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        private static bool IsValidPlan(int planDays)
        {
            return planDays switch
            {
                7 => true,
                15 => true,
                30 => true,
                45 => true,
                50 => true,
                _ => false
            };
        }
    }
}
