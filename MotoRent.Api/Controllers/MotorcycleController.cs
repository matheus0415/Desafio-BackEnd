using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Application.DTOs;
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
    [Route("motos")]
    public class MotorcycleController : ControllerBase
    {
        private readonly MotoRentDbContext _context;
        private readonly MotoRent.Application.Services.MotorcycleService _motorcycleService;
        private readonly IMessagingService _messagingService;

        public MotorcycleController(MotoRentDbContext context, IMessagingService messagingService)
        {
            _context = context;
            _messagingService = messagingService;
            _motorcycleService = new MotoRent.Application.Services.MotorcycleService(_context);
        }

        /// <summary>
        /// Cadastrar moto.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar uma nova moto", Description = "Cria uma nova moto na plataforma.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Cadastrar(CreateMotorcycleDto dto)
        {
            if (await _motorcycleService.LicensePlateExistsAsync(dto.LicensePlate))
                return BadRequest("Placa já cadastrada.");

            var motorcycle = await _motorcycleService.CreateAsync(dto);

            await _messagingService.PublishMotorcycleRegisteredAsync(motorcycle.Id, motorcycle.LicensePlate, motorcycle.Year);
            await _messagingService.PublishYear2024NotificationAsync(motorcycle.Id, motorcycle.LicensePlate, motorcycle.Year);

            return StatusCode(201);
        }

        /// <summary>
        /// Consultar motos existentes.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Consultar motos existentes", Description = "Retorna todas as motos com filtros opcionais.")]
        [ProducesResponseType(typeof(IEnumerable<MotorcycleListItemDto>), 200)]
        public async Task<ActionResult<IEnumerable<MotorcycleListItemDto>>> Listar([FromQuery] string? placa)
        {
            List<Motorcycle> motorcycles = await _motorcycleService.GetAllAsync(placa);
            IEnumerable<MotorcycleListItemDto> dtos = motorcycles.Select(m => new MotorcycleListItemDto
            {
                Identifier = m.Identifier,
                Year = m.Year,
                Model = m.Model,
                LicensePlate = m.LicensePlate
            });
            return Ok(dtos);
        }

        /// <summary>
        /// Modificar a placa de uma moto.
        /// </summary>
        [HttpPut("{id}/placa")]
        [SwaggerOperation(Summary = "Modificar a placa de uma moto", Description = "Atualiza apenas a placa da moto.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AtualizarPlaca(Guid id, UpdateMotorcycleDto dto)
        {
            if (dto.LicensePlate != null && await _motorcycleService.LicensePlateExistsAsync(dto.LicensePlate, id))
                return BadRequest("Nova placa já cadastrada.");
            var updated = await _motorcycleService.UpdateLicensePlateAsync(id, dto.LicensePlate);
            if (!updated) return BadRequest("Moto não encontrada.");
            return StatusCode(201, "Placa modificada com sucesso");
        }

        /// <summary>
        /// Buscar moto por ID.
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Consultar motos existentes por ID", Description = "Retorna uma moto específica pelo ID.")]
        [ProducesResponseType(typeof(MotorcycleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MotorcycleDto>> BuscarPorId(Guid id)
        {
            var motorcycle = await _motorcycleService.GetByIdAsync(id);
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
        /// Remover uma moto.
        /// </summary>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remover uma moto", Description = "Remove uma moto pelo ID.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            bool deleted = await _motorcycleService.DeleteAsync(id);
            if (!deleted)
                return BadRequest("Não é possível remover uma moto com histórico de aluguel ou inexistente.");
            return StatusCode(201, "Moto removida com sucesso");
        }

    }
}
