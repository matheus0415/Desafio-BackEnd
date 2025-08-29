using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.DTOs;
using MotoRent.Domain.Entities;
using MotoRent.Domain.Services;
using MotoRent.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Annotations;

namespace MotoRent.Api.Controllers
{
    /// <summary>
    /// Operations for Motorcycle Rental.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RentalController : ControllerBase
    {
        private readonly MotoRentDbContext _context;
        private readonly IRentalCalculationService _calculationService;

        public RentalController(MotoRentDbContext context, IRentalCalculationService calculationService)
        {
            _context = context;
            _calculationService = calculationService;
        }

        /// <summary>
        /// Get all rentals.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Get all rentals", Description = "Returns a list of all rentals.")]
        [ProducesResponseType(typeof(IEnumerable<RentalDto>), 200)]
        public async Task<ActionResult<IEnumerable<RentalDto>>> GetAll()
        {
            var rentals = await _context.Rentals
                .Include(l => l.Courier)
                .Include(l => l.Motorcycle)
                .Select(l => new RentalDto
                {
                    Id = l.Id,
                    CourierId = l.CourierId,
                    MotorcycleId = l.MotorcycleId,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    ExpectedEndDate = l.ExpectedEndDate,
                    ReturnDate = l.ReturnDate,
                    PlanDays = l.PlanDays,
                    DailyRate = l.DailyRate,
                    TotalAmount = l.TotalAmount,
                    FineAmount = l.FineAmount,
                    AdditionalAmount = l.AdditionalAmount,
                    Status = l.Status,
                    RegistrationDate = l.RegistrationDate,
                    CourierName = l.Courier.Name,
                    MotorcycleModel = l.Motorcycle.Model,
                    MotorcycleLicensePlate = l.Motorcycle.LicensePlate
                })
                .ToListAsync();

            return Ok(rentals);
        }

        /// <summary>
        /// Get a rental by ID.
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get rental by ID", Description = "Returns a specific rental by ID.")]
        [ProducesResponseType(typeof(RentalDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RentalDto>> GetById(Guid id)
        {
            var rental = await _context.Rentals
                .Include(l => l.Courier)
                .Include(l => l.Motorcycle)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (rental == null) return NotFound();

            return Ok(new RentalDto
            {
                Id = rental.Id,
                CourierId = rental.CourierId,
                MotorcycleId = rental.MotorcycleId,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ReturnDate = rental.ReturnDate,
                PlanDays = rental.PlanDays,
                DailyRate = rental.DailyRate,
                TotalAmount = rental.TotalAmount,
                FineAmount = rental.FineAmount,
                AdditionalAmount = rental.AdditionalAmount,
                Status = rental.Status,
                RegistrationDate = rental.RegistrationDate,
                CourierName = rental.Courier.Name,
                MotorcycleModel = rental.Motorcycle.Model,
                MotorcycleLicensePlate = rental.Motorcycle.LicensePlate
            });
        }

        /// <summary>
        /// Create a new rental.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create rental", Description = "Creates a new motorcycle rental.")]
        [ProducesResponseType(typeof(RentalDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RentalDto>> Create(CreateRentalDto dto)
        {
            // Validate plan days
            if (!IsValidPlan(dto.PlanDays))
                return BadRequest("Invalid plan days. Use 7, 15, 30, 45 or 50 days.");

            // Check if motorcycle exists and is available
            var motorcycle = await _context.Motorcycles.FindAsync(dto.MotorcycleId);
            if (motorcycle == null)
                return BadRequest("Motorcycle not found.");

            if (!motorcycle.Available)
                return BadRequest("Motorcycle is not available for rental.");

            // Check if courier is enabled for category A
            var courier = await _context.Couriers.FindAsync(dto.CourierId);
            if (courier == null)
                return BadRequest("Courier not found.");

            if (!courier.Enabled)
                return BadRequest("Courier is not enabled.");

            if (!courier.LicenseType.Contains("A"))
                return BadRequest("Courier must have category A license to rent motorcycles.");

            // Calculate dates
            var startDate = DateTime.Today.AddDays(1); // First day after creation date
            var endDate = startDate.AddDays(dto.PlanDays);
            var expectedEndDate = endDate;

            // Calculate values
            var dailyRate = _calculationService.CalculateDailyRate(dto.PlanDays);
            var totalAmount = _calculationService.CalculateTotalAmount(dto.PlanDays, startDate, endDate);

            var rental = new Rental
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

            // Mark motorcycle as unavailable
            motorcycle.Available = false;

            await _context.SaveChangesAsync();

            var rentalDto = new RentalDto
            {
                Id = rental.Id,
                CourierId = rental.CourierId,
                MotorcycleId = rental.MotorcycleId,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                PlanDays = rental.PlanDays,
                DailyRate = rental.DailyRate,
                TotalAmount = rental.TotalAmount,
                Status = rental.Status,
                RegistrationDate = rental.RegistrationDate
            };

            return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rentalDto);
        }

        /// <summary>
        /// Complete a rental.
        /// </summary>
        [HttpPut("{id}/complete")]
        [SwaggerOperation(Summary = "Complete rental", Description = "Completes a rental and calculates final values.")]
        [ProducesResponseType(typeof(RentalResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RentalResponseDto>> CompleteRental(Guid id, CompleteRentalDto dto)
        {
            var rental = await _context.Rentals
                .Include(l => l.Motorcycle)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (rental == null) return NotFound();

            if (rental.Status != "Active")
                return BadRequest("Rental is not active.");

            if (dto.ReturnDate < rental.StartDate)
                return BadRequest("Return date cannot be before start date.");

            // Calculate final values
            var fineAmount = _calculationService.CalculateFine(rental.PlanDays, rental.ExpectedEndDate, dto.ReturnDate);
            var additionalAmount = _calculationService.CalculateAdditionalAmount(rental.ExpectedEndDate, dto.ReturnDate);

            // Calculate total amount of effective days
            var effectiveDays = (dto.ReturnDate - rental.StartDate).Days;
            var totalDaysAmount = rental.DailyRate * effectiveDays;

            var finalTotalAmount = totalDaysAmount + fineAmount + additionalAmount;

            // Update rental
            rental.ReturnDate = dto.ReturnDate;
            rental.FineAmount = fineAmount;
            rental.AdditionalAmount = additionalAmount;
            rental.TotalAmount = finalTotalAmount;
            rental.Status = "Completed";

            // Mark motorcycle as available
            rental.Motorcycle.Available = true;

            await _context.SaveChangesAsync();

            var response = new RentalResponseDto
            {
                TotalAmount = finalTotalAmount,
                FineAmount = fineAmount,
                AdditionalAmount = additionalAmount,
                Message = "Rental completed successfully."
            };

            return Ok(response);
        }

        /// <summary>
        /// Get rentals by courier.
        /// </summary>
        [HttpGet("courier/{courierId}")]
        [SwaggerOperation(Summary = "Get rentals by courier", Description = "Returns all rentals of a specific courier.")]
        [ProducesResponseType(typeof(IEnumerable<RentalDto>), 200)]
        public async Task<ActionResult<IEnumerable<RentalDto>>> GetByCourier(Guid courierId)
        {
            var rentals = await _context.Rentals
                .Include(l => l.Courier)
                .Include(l => l.Motorcycle)
                .Where(l => l.CourierId == courierId)
                .Select(l => new RentalDto
                {
                    Id = l.Id,
                    CourierId = l.CourierId,
                    MotorcycleId = l.MotorcycleId,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    ExpectedEndDate = l.ExpectedEndDate,
                    ReturnDate = l.ReturnDate,
                    PlanDays = l.PlanDays,
                    DailyRate = l.DailyRate,
                    TotalAmount = l.TotalAmount,
                    FineAmount = l.FineAmount,
                    AdditionalAmount = l.AdditionalAmount,
                    Status = l.Status,
                    RegistrationDate = l.RegistrationDate,
                    CourierName = l.Courier.Name,
                    MotorcycleModel = l.Motorcycle.Model,
                    MotorcycleLicensePlate = l.Motorcycle.LicensePlate
                })
                .ToListAsync();

            return Ok(rentals);
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
