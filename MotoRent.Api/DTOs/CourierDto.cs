using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace MotoRent.Api.DTOs
{
    public class CourierDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter 14 dígitos.")]
        public string CNPJ { get; set; } = null!;
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Número da CNH deve conter 11 dígitos.")]
        public string LicenseNumber { get; set; } = null!;
        [Required]
        [RegularExpression(@"^(A|B|A\+B)$", ErrorMessage = "Tipo de CNH deve ser 'A', 'B' ou 'A+B'.")]
        public string LicenseType { get; set; } = null!;
        public string? LicenseImage { get; set; }
        public bool Enabled { get; set; }
        public DateTime RegistrationDate { get; set; }
    }


    public class UpdateCourierDto
    {
        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
        [RegularExpression(@"^(A|B|A\+B)$", ErrorMessage = "Tipo de CNH deve ser 'A', 'B' ou 'A+B'.")]
        public string? LicenseType { get; set; }
    }

    public class UpdateLicenseImageDto
    {
        [Required]
        public string imagem_cnh { get; set; } = null!;
    }
}
