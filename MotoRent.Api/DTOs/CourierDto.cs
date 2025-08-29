namespace MotoRent.Api.DTOs
{
    public class CourierDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string CNPJ { get; set; }
        public DateTime BirthDate { get; set; }
        public required string LicenseNumber { get; set; }
        public required string LicenseType { get; set; }
        public string? LicenseImage { get; set; }
        public bool Enabled { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class CreateCourierDto
    {
        public required string Name { get; set; }
        public required string CNPJ { get; set; }
        public DateTime BirthDate { get; set; }
        public required string LicenseNumber { get; set; }
        public required string LicenseType { get; set; }
    }

    public class UpdateCourierDto
    {
        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? LicenseType { get; set; }
    }

    public class UpdateLicenseImageDto
    {
        public IFormFile LicenseImage { get; set; } = null!;
    }
}
