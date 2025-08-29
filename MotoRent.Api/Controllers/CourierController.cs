using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRent.Domain.Entities;
using MotoRent.Domain.ValueObjects;
using MotoRent.Infrastructure.Persistence;
using MotoRent.Infrastructure.Services;
using Swashbuckle.AspNetCore.Annotations;
using MotoRent.Api.DTOs;
using MotoRent.Application.DTOs;
using MotoRent.Application.DTOs.Responses;

namespace MotoRent.Api.Controllers
{
    /// <summary>
    /// CRUD operations for Courier entity.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CourierController : ControllerBase
    {
    private readonly MotoRentDbContext _context;
    private readonly IStorageService _storageService;
    private readonly MotoRent.Application.Services.CourierService _courierService;

        public CourierController(MotoRentDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
            _courierService = new MotoRent.Application.Services.CourierService(_context);
        }


        /// <summary>
        /// Cadastrar Entregador.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar Entregador", Description = "Creates a new courier in the platform.")]
        [ProducesResponseType(typeof(CreateCourierResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<ActionResult<CourierDto>> Create(CreateCourierDto dto)
        {
            CreateCourierResponse response = await _courierService.CreateCourierAsync(dto);
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        /// <summary>
        /// Enviar foto de CNH.
        /// </summary>
        [HttpPost("{id}/cnh")]
        [SwaggerOperation(Summary = "Enviar foto de CNH", Description = "Updates the courier's license image.")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> UpdateLicenseImage(Guid id, UpdateLicenseImageDto dto)
        {
            using var stream = dto.LicenseImage.OpenReadStream();
            UpdateLicenseImageResponse response = await _courierService.UpdateLicenseImageAsync(
                id,
                stream,
                dto.LicenseImage.ContentType,
                _storageService.DeleteImageAsync,
                _storageService.SaveImageAsync
            );
            if (response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

    }
}
