namespace MotoRent.Domain.Entities
{
    public class Motorcycle
    {
        public Guid Id { get; set; }
        public required string Identifier { get; set; }
        public int Year { get; set; }
        public required string Model { get; set; }
        public required string LicensePlate { get; set; }
        public bool Available { get; set; } = true;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        // Relationships
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
