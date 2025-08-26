namespace MotoRent.Domain.Entities
{
    public class Courier
    {
        public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Document { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
