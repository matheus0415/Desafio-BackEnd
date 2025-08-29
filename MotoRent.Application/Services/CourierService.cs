using MotoRent.Application.DTOs;
using MotoRent.Application.DTOs.Responses;
using MotoRent.Domain.Entities;
using MotoRent.Domain.ValueObjects;
using MotoRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MotoRent.Application.Services
{
    public class CourierService
    {
        private readonly MotoRentDbContext _context;

        public CourierService(MotoRentDbContext context)
        {
            _context = context;
        }

    public async Task<CreateCourierResponse> CreateCourierAsync(CreateCourierDto dto)
        {
            if (!LicenseTypeValidator.IsValid(dto.LicenseType))
                return new CreateCourierResponse { Success = false, Mensagem = "Dados inválidos" };

            if (await _context.Couriers.AnyAsync(e => e.CNPJ == dto.CNPJ))
                return new CreateCourierResponse { Success = false, Mensagem = "Dados inválidos" };

            if (await _context.Couriers.AnyAsync(e => e.LicenseNumber == dto.LicenseNumber))
                return new CreateCourierResponse { Success = false, Mensagem = "Dados inválidos" };

            Courier courier = new Courier
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

            return new CreateCourierResponse { Success = true };
        }

    public async Task<UpdateLicenseImageResponse> UpdateLicenseImageAsync(
        Guid id,
        Stream imageStream,
        string contentType,
        Func<string, Task> deleteImageAsync,
        Func<Stream, string, string, Task<string>> saveImageAsync)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null)
                return new UpdateLicenseImageResponse { Success = false, Mensagem = "Dados inválidos" };

            try
            {
                var fileName = $"license_{courier.Id}";
                var savedName = await saveImageAsync(imageStream, fileName, contentType);

                // Delete previous image if exists
                if (!string.IsNullOrEmpty(courier.LicenseImage))
                {
                    await deleteImageAsync(courier.LicenseImage!);
                }

                courier.LicenseImage = savedName;
                await _context.SaveChangesAsync();

                return new UpdateLicenseImageResponse { Success = true };
            }
            catch (ArgumentException ex)
            {
                return new UpdateLicenseImageResponse { Success = false, Mensagem = ex.Message };
            }
        }
    }
}
