namespace MotoRent.Api.DTOs
{
    public class CourierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}
