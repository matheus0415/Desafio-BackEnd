using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MotoRent.Infrastructure.Persistence
{
    public class MotoRentDbContextFactory : IDesignTimeDbContextFactory<MotoRentDbContext>
    {
        public MotoRentDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MotoRentDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=motorrent;Username=admin;Password=admin");
            return new MotoRentDbContext(optionsBuilder.Options);
        }
    }
}
