namespace MotoRent.Application.DTOs
{
    public class MotorcycleListItemDto
    {
        public string Identifier { get; set; } = null!;
        public int Year { get; set; }
        public string Model { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;
    }
}