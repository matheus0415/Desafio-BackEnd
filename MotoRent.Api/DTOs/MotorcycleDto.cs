namespace MotoRent.Api.DTOs
{
    public class MotorcycleDto
    {
        public Guid Id { get; set; }
        public required string Identifier { get; set; }
        public int Year { get; set; }
        public required string Model { get; set; }
        public required string LicensePlate { get; set; }
        public bool Available { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class CreateMotorcycleDto
    {
        public required string Identifier { get; set; }
        public int Year { get; set; }
        public required string Model { get; set; }
        public required string LicensePlate { get; set; }
    }

    public class UpdateMotorcycleDto
    {
        public string? LicensePlate { get; set; }
    }

    public class FilterMotorcycleDto
    {
        public string? LicensePlate { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}
