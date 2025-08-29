using Microsoft.EntityFrameworkCore;
using MotoRent.Domain.Entities;

namespace MotoRent.Infrastructure.Persistence
{
    public class MotoRentDbContext : DbContext
    {
        public MotoRentDbContext(DbContextOptions<MotoRentDbContext> options) : base(options) { }

        public DbSet<Courier> Couriers { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<MotorcycleEvent> MotorcycleEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Courier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CNPJ).IsRequired().HasMaxLength(18);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.LicenseType).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                
                entity.HasIndex(e => e.CNPJ).IsUnique();
                entity.HasIndex(e => e.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<Motorcycle>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.LicensePlate).IsRequired().HasMaxLength(8);
                entity.Property(m => m.Model).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Identifier).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(m => m.LicensePlate).IsUnique();
            });

            modelBuilder.Entity<Rental>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.DailyRate).HasPrecision(10, 2);
                entity.Property(l => l.TotalAmount).HasPrecision(10, 2);
                entity.Property(l => l.FineAmount).HasPrecision(10, 2);
                entity.Property(l => l.AdditionalAmount).HasPrecision(10, 2);
                entity.Property(l => l.Status).IsRequired().HasMaxLength(20);
                
                entity.HasOne(l => l.Courier)
                      .WithMany(e => e.Rentals)
                      .HasForeignKey(l => l.CourierId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(l => l.Motorcycle)
                      .WithMany(m => m.Rentals)
                      .HasForeignKey(l => l.MotorcycleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MotorcycleEvent>(entity =>
            {
                entity.HasKey(em => em.Id);
                entity.Property(em => em.EventType).IsRequired().HasMaxLength(50);
                entity.Property(em => em.AdditionalData).HasMaxLength(500);
                
                entity.HasOne(em => em.Motorcycle)
                      .WithMany()
                      .HasForeignKey(em => em.MotorcycleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
