using System.ComponentModel.DataAnnotations;
namespace MotoRent.Api.DTOs
{
    public class MotorcycleDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Identifier { get; set; } = null!;
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
        [Required]
        public string Model { get; set; } = null!;
        [Required]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$|^[A-Z]{3}\d[A-Z]\d{2}$", ErrorMessage = "Formato de placa inválido.")]
        public string LicensePlate { get; set; } = null!;
        public bool Available { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class CreateMotorcycleDto
    {
        [Required]
        public string Identifier { get; set; } = null!;
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
        [Required]
        public string Model { get; set; } = null!;
        [Required]
        [RegularExpression(@"^[A-Z]{3}-\d{4}$|^[A-Z]{3}\d[A-Z]\d{2}$", ErrorMessage = "Formato de placa inválido.")]
        public string LicensePlate { get; set; } = null!;
    }

    public class UpdateMotorcycleDto
    {
        [RegularExpression(@"^[A-Z]{3}-\d{4}$|^[A-Z]{3}\d[A-Z]\d{2}$", ErrorMessage = "Formato de placa inválido.")]
        public string? LicensePlate { get; set; }
    }

    public class FilterMotorcycleDto
    {
        public string? LicensePlate { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}
