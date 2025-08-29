namespace MotoRent.Domain.Entities
{
    public class Courier
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string CNPJ { get; set; }
        public DateTime BirthDate { get; set; }
        public required string LicenseNumber { get; set; }
        public required string LicenseType { get; set; } // A, B, or A+B
        public string? LicenseImage { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        // Relationships
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
