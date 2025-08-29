using MotoRent.Domain.Entities;
using MotoRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MotoRent.Application.DTOs;

namespace MotoRent.Application.Services
{
    public class MotorcycleService
    {
        private readonly MotoRentDbContext _context;

        public MotorcycleService(MotoRentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LicensePlateExistsAsync(string licensePlate, Guid? excludeId = null)
        {
            return await _context.Motorcycles.AnyAsync(m => m.LicensePlate == licensePlate && (!excludeId.HasValue || m.Id != excludeId));
        }

        public async Task<Motorcycle?> GetByIdAsync(Guid id)
        {
            return await _context.Motorcycles.Include(m => m.Rentals).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Motorcycle>> GetAllAsync(string? licensePlate)
        {
            var query = _context.Motorcycles.AsQueryable();
            if (!string.IsNullOrEmpty(licensePlate))
                query = query.Where(m => m.LicensePlate.Contains(licensePlate));
            return await query.ToListAsync();
        }

        public async Task<Motorcycle> CreateAsync(CreateMotorcycleDto dto)
        {
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
            return motorcycle;
        }

        public async Task<bool> UpdateLicensePlateAsync(Guid id, string? newLicensePlate)
        {
            var motorcycle = await _context.Motorcycles.FindAsync(id);
            if (motorcycle == null) return false;
            if (newLicensePlate != null)
            {
                motorcycle.LicensePlate = newLicensePlate;
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var motorcycle = await _context.Motorcycles.Include(m => m.Rentals).FirstOrDefaultAsync(m => m.Id == id);
            if (motorcycle == null) return false;
            if (motorcycle.Rentals.Any()) return false;
            _context.Motorcycles.Remove(motorcycle);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
