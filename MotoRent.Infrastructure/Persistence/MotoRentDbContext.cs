using Microsoft.EntityFrameworkCore;
using MotoRent.Domain.Entities;

namespace MotoRent.Infrastructure.Persistence
{
    public class MotoRentDbContext : DbContext
    {
        public MotoRentDbContext(DbContextOptions<MotoRentDbContext> options) : base(options) { }

    public DbSet<Courier> Couriers { get; set; }
    }
}
