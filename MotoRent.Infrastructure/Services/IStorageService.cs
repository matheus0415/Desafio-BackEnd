namespace MotoRent.Infrastructure.Services
{
    public interface IStorageService
    {
    Task<string> SaveImageAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteImageAsync(string fileName);
    Task<byte[]?> GetImageAsync(string fileName);
    }
}
