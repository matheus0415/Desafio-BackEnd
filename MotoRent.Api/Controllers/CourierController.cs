
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.DTOs;
using MotoRent.Domain.Entities;
using MotoRent.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Annotations;

namespace MotoRent.Api.Controllers
{
    /// <summary>
    /// CRUD operations for Courier entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly MotoRentDbContext _context;

        public CourierController(MotoRentDbContext context)
        {
            _context = context;
        }

    /// <summary>
    /// Get all couriers.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all couriers", Description = "Returns a list of all couriers.")]
    [ProducesResponseType(typeof(IEnumerable<CourierDto>), 200)]
    public async Task<ActionResult<IEnumerable<CourierDto>>> GetAll()
        {
            var couriers = await _context.Couriers
                .Select(c => new CourierDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Document = c.Document,
                    BirthDate = c.BirthDate
                })
                .ToListAsync();
            return Ok(couriers);
        }

    /// <summary>
    /// Get a courier by ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get courier by ID", Description = "Returns a single courier by ID.")]
    [ProducesResponseType(typeof(CourierDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CourierDto>> GetById(Guid id)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();
            return Ok(new CourierDto
            {
                Id = courier.Id,
                Name = courier.Name,
                Document = courier.Document,
                BirthDate = courier.BirthDate
            });
        }

    /// <summary>
    /// Create a new courier.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create courier", Description = "Creates a new courier.")]
    [ProducesResponseType(typeof(CourierDto), 201)]
    public async Task<ActionResult<CourierDto>> Create(CourierDto dto)
        {
            var courier = new Courier
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Document = dto.Document,
                BirthDate = dto.BirthDate
            };
            _context.Couriers.Add(courier);
            await _context.SaveChangesAsync();
            dto.Id = courier.Id;
            return CreatedAtAction(nameof(GetById), new { id = courier.Id }, dto);
        }

    /// <summary>
    /// Update an existing courier.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update courier", Description = "Updates an existing courier.")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, CourierDto dto)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();
            courier.Name = dto.Name;
            courier.Document = dto.Document;
            courier.BirthDate = dto.BirthDate;
            await _context.SaveChangesAsync();
            return NoContent();
        }

    /// <summary>
    /// Delete a courier by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete courier", Description = "Deletes a courier by ID.")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();
            _context.Couriers.Remove(courier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
