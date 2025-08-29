using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Application.DTOs;
using MotoRent.Domain.Entities;
using MotoRent.Domain.Services;
using MotoRent.Api.DTOs;
using MotoRent.Application.Services;
using MotoRent.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Annotations;

namespace MotoRent.Api.Controllers
{
    /// <summary>
    /// Operations for Motorcycle Rental.
    /// </summary>
    [ApiController]
    [Route("locacao")]
    public class RentalController : ControllerBase
        {
            private readonly RentalService _rentalService;
        
        public RentalController(RentalService rentalService)
        {
            _rentalService = rentalService;
        }

        /// <summary>
        /// Alugar uma moto
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Alugar uma moto", Description = "Creates a new motorcycle rental.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateRent([FromBody] CreateRentalDto dto)
        {
            (bool Success, string? ErrorMessage) result = await _rentalService.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return StatusCode(201, "Aluguel criado com sucesso");
        }

        /// <summary>
        /// Consultar locação por id
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Consultar locação por ID", Description = "Returns a specific rental by ID.")]
        [ProducesResponseType(typeof(RentalDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RentalDto>> GetRentById(Guid id)
        {
            Rental? rental = await _rentalService.GetByIdAsync(id);
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
                CourierName = rental.Courier?.Name,
                MotorcycleModel = rental.Motorcycle?.Model,
                MotorcycleLicensePlate = rental.Motorcycle?.LicensePlate
            });
        }

        /// <summary>
        /// Devolução de moto (finalizar aluguel)
        /// </summary>
        [HttpPut("{id}/devolucao")]
        [SwaggerOperation(Summary = "Informar data de devolução e calcular o valor", Description = "Completes a rental and calculates final values.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Devolver(Guid id, [FromBody] CompleteRentalDto dto)
        {
            (bool Success, string? ErrorMessage) result = await _rentalService.CompleteAsync(id, dto.ReturnDate);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return StatusCode(201, "Aluguel finalizado com sucesso");
        }

    
    }
}
