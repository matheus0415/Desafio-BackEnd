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
    /// CRUD operations for Motorcycle entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MotorcycleController : ControllerBase
    {
        private readonly MotoRentDbContext _context;
        private readonly IMessagingService _messagingService;

        public MotorcycleController(MotoRentDbContext context, IMessagingService messagingService)
        {
            _context = context;
            _messagingService = messagingService;
        }

        /// <summary>
        /// Get all motorcycles with optional filters.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Get all motorcycles", Description = "Returns a list of all motorcycles with optional filters.")]
        [ProducesResponseType(typeof(IEnumerable<MotorcycleDto>), 200)]
        public async Task<ActionResult<IEnumerable<MotorcycleDto>>> GetAll([FromQuery] FilterMotorcycleDto filter)
        {
            var query = _context.Motorcycles.AsQueryable();

            if (!string.IsNullOrEmpty(filter.LicensePlate))
                query = query.Where(m => m.LicensePlate.Contains(filter.LicensePlate));

            if (!string.IsNullOrEmpty(filter.Model))
                query = query.Where(m => m.Model.Contains(filter.Model));

            if (filter.Year.HasValue)
                query = query.Where(m => m.Year == filter.Year.Value);

            var motorcycles = await query
                .Select(m => new MotorcycleDto
                {
                    Id = m.Id,
                    Identifier = m.Identifier,
                    Year = m.Year,
                    Model = m.Model,
                    LicensePlate = m.LicensePlate,
                    Available = m.Available,
                    RegistrationDate = m.RegistrationDate
                })
                .ToListAsync();

            return Ok(motorcycles);
        }

        /// <summary>
        /// Get a motorcycle by ID.
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get motorcycle by ID", Description = "Returns a specific motorcycle by ID.")]
        [ProducesResponseType(typeof(MotorcycleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MotorcycleDto>> GetById(Guid id)
        {
            var motorcycle = await _context.Motorcycles.FindAsync(id);
            if (motorcycle == null) return NotFound();

            return Ok(new MotorcycleDto
            {
                Id = motorcycle.Id,
                Identifier = motorcycle.Identifier,
                Year = motorcycle.Year,
                Model = motorcycle.Model,
                LicensePlate = motorcycle.LicensePlate,
                Available = motorcycle.Available,
                RegistrationDate = motorcycle.RegistrationDate
            });
        }

        /// <summary>
        /// Create a new motorcycle.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create motorcycle", Description = "Creates a new motorcycle in the platform.")]
        [ProducesResponseType(typeof(MotorcycleDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MotorcycleDto>> Create(CreateMotorcycleDto dto)
        {
            // Check if license plate already exists
            if (await _context.Motorcycles.AnyAsync(m => m.LicensePlate == dto.LicensePlate))
                return BadRequest("License plate already registered.");

            var motorcycle = new Motorcycle
            {
                Id = Guid.NewGuid(),
                Identifier = dto.Identifier,
                Year = dto.Year,
                Model = dto.Model,
                LicensePlate = dto.LicensePlate,
                Available = true,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Motorcycles.Add(motorcycle);
            await _context.SaveChangesAsync();

            // Publish motorcycle registered event
            await _messagingService.PublishMotorcycleRegisteredAsync(motorcycle.Id, motorcycle.LicensePlate, motorcycle.Year);
            
            // Publish special notification for year 2024
            await _messagingService.PublishYear2024NotificationAsync(motorcycle.Id, motorcycle.LicensePlate, motorcycle.Year);

            var motorcycleDto = new MotorcycleDto
            {
                Id = motorcycle.Id,
                Identifier = motorcycle.Identifier,
                Year = motorcycle.Year,
                Model = motorcycle.Model,
                LicensePlate = motorcycle.LicensePlate,
                Available = motorcycle.Available,
                RegistrationDate = motorcycle.RegistrationDate
            };

            return CreatedAtAction(nameof(GetById), new { id = motorcycle.Id }, motorcycleDto);
        }

        /// <summary>
        /// Update an existing motorcycle (license plate only).
        /// </summary>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update motorcycle", Description = "Updates only the license plate of an existing motorcycle.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, UpdateMotorcycleDto dto)
        {
            var motorcycle = await _context.Motorcycles.FindAsync(id);
            if (motorcycle == null) return NotFound();

            if (dto.LicensePlate != null)
            {
                // Check if new license plate already exists
                if (await _context.Motorcycles.AnyAsync(m => m.Id != id && m.LicensePlate == dto.LicensePlate))
                    return BadRequest("New license plate already registered.");

                motorcycle.LicensePlate = dto.LicensePlate;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a motorcycle by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete motorcycle", Description = "Deletes a motorcycle by ID.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var motorcycle = await _context.Motorcycles
                .Include(m => m.Rentals)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (motorcycle == null) return NotFound();

            // Check if has rentals
            if (motorcycle.Rentals.Any())
                return BadRequest("Cannot delete a motorcycle that has rental history.");

            _context.Motorcycles.Remove(motorcycle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Get available motorcycles for rental.
        /// </summary>
        [HttpGet("available")]
        [SwaggerOperation(Summary = "Get available motorcycles", Description = "Returns a list of motorcycles available for rental.")]
        [ProducesResponseType(typeof(IEnumerable<MotorcycleDto>), 200)]
        public async Task<ActionResult<IEnumerable<MotorcycleDto>>> GetAvailable()
        {
            var motorcycles = await _context.Motorcycles
                .Where(m => m.Available)
                .Select(m => new MotorcycleDto
                {
                    Id = m.Id,
                    Identifier = m.Identifier,
                    Year = m.Year,
                    Model = m.Model,
                    LicensePlate = m.LicensePlate,
                    Available = m.Available,
                    RegistrationDate = m.RegistrationDate
                })
                .ToListAsync();

            return Ok(motorcycles);
        }
    }
}
