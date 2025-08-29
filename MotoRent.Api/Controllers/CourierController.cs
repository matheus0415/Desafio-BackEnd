using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Api.DTOs;
using MotoRent.Domain.Entities;
using MotoRent.Infrastructure.Persistence;
using MotoRent.Infrastructure.Services;
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
        private readonly IStorageService _storageService;

        public CourierController(MotoRentDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        /// <summary>
        /// Get all couriers.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Get all couriers", Description = "Returns a list of all registered couriers.")]
        [ProducesResponseType(typeof(IEnumerable<CourierDto>), 200)]
        public async Task<ActionResult<IEnumerable<CourierDto>>> GetAll()
        {
            var couriers = await _context.Couriers
                .Select(e => new CourierDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    CNPJ = e.CNPJ,
                    BirthDate = e.BirthDate,
                    LicenseNumber = e.LicenseNumber,
                    LicenseType = e.LicenseType,
                    LicenseImage = e.LicenseImage,
                    Enabled = e.Enabled,
                    RegistrationDate = e.RegistrationDate
                })
                .ToListAsync();
            return Ok(couriers);
        }

        /// <summary>
        /// Get a courier by ID.
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get courier by ID", Description = "Returns a specific courier by ID.")]
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
                CNPJ = courier.CNPJ,
                BirthDate = courier.BirthDate,
                LicenseNumber = courier.LicenseNumber,
                LicenseType = courier.LicenseType,
                LicenseImage = courier.LicenseImage,
                Enabled = courier.Enabled,
                RegistrationDate = courier.RegistrationDate
            });
        }

        /// <summary>
        /// Create a new courier.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Create courier", Description = "Creates a new courier in the platform.")]
        [ProducesResponseType(typeof(CourierDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<CourierDto>> Create(CreateCourierDto dto)
        {
            // Validate license type
            if (!IsValidLicenseType(dto.LicenseType))
                return BadRequest("Invalid license type. Use A, B or A+B.");

            // Check if CNPJ already exists
            if (await _context.Couriers.AnyAsync(e => e.CNPJ == dto.CNPJ))
                return BadRequest("CNPJ already registered.");

            // Check if license number already exists
            if (await _context.Couriers.AnyAsync(e => e.LicenseNumber == dto.LicenseNumber))
                return BadRequest("License number already registered.");

            var courier = new Courier
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CNPJ = dto.CNPJ,
                BirthDate = dto.BirthDate,
                LicenseNumber = dto.LicenseNumber,
                LicenseType = dto.LicenseType,
                Enabled = true,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Couriers.Add(courier);
            await _context.SaveChangesAsync();

            var courierDto = new CourierDto
            {
                Id = courier.Id,
                Name = courier.Name,
                CNPJ = courier.CNPJ,
                BirthDate = courier.BirthDate,
                LicenseNumber = courier.LicenseNumber,
                LicenseType = courier.LicenseType,
                LicenseImage = courier.LicenseImage,
                Enabled = courier.Enabled,
                RegistrationDate = courier.RegistrationDate
            };

            return CreatedAtAction(nameof(GetById), new { id = courier.Id }, courierDto);
        }

        /// <summary>
        /// Update an existing courier.
        /// </summary>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update courier", Description = "Updates an existing courier.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, UpdateCourierDto dto)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();

            if (dto.Name != null)
                courier.Name = dto.Name;
            
            if (dto.BirthDate.HasValue)
                courier.BirthDate = dto.BirthDate.Value;
            
            if (dto.LicenseType != null)
            {
                if (!IsValidLicenseType(dto.LicenseType))
                    return BadRequest("Invalid license type. Use A, B or A+B.");
                courier.LicenseType = dto.LicenseType;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Update courier's license image.
        /// </summary>
        [HttpPut("{id}/license-image")]
        [SwaggerOperation(Summary = "Update license image", Description = "Updates the courier's license image.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateLicenseImage(Guid id, UpdateLicenseImageDto dto)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();

            try
            {
                var fileName = $"license_{courier.Id}";
                using var stream = dto.LicenseImage.OpenReadStream();
                var savedName = await _storageService.SaveImageAsync(stream, fileName, dto.LicenseImage.ContentType);
                
                // Delete previous image if exists
                if (!string.IsNullOrEmpty(courier.LicenseImage))
                {
                    await _storageService.DeleteImageAsync(courier.LicenseImage);
                }
                
                courier.LicenseImage = savedName;
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a courier by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete courier", Description = "Deletes a courier by ID.")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var courier = await _context.Couriers
                .Include(e => e.Rentals)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (courier == null) return NotFound();

            // Check if has active rentals
            if (courier.Rentals.Any(l => l.Status == "Active"))
                return BadRequest("Cannot delete a courier with active rentals.");

            _context.Couriers.Remove(courier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Get courier's license image.
        /// </summary>
        [HttpGet("{id}/license-image")]
        [SwaggerOperation(Summary = "Get license image", Description = "Returns the courier's license image.")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLicenseImage(Guid id)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null || string.IsNullOrEmpty(courier.LicenseImage))
                return NotFound();

            var image = await _storageService.GetImageAsync(courier.LicenseImage);
            if (image == null)
                return NotFound();

            var extension = Path.GetExtension(courier.LicenseImage).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return File(image, contentType);
        }

        private static bool IsValidLicenseType(string licenseType)
        {
            return licenseType switch
            {
                "A" => true,
                "B" => true,
                "A+B" => true,
                _ => false
            };
        }
    }
}
