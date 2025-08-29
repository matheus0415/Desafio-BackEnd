using MotoRent.Infrastructure.Services;

namespace MotoRent.Infrastructure.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _baseDirectory;
        private readonly string _imagesDirectory;

        public LocalStorageService()
        {
            _baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            _imagesDirectory = Path.Combine(_baseDirectory, "Images");

            if (!Directory.Exists(_imagesDirectory))
            {
                Directory.CreateDirectory(_imagesDirectory);
            }
        }

        public async Task<string> SaveImageAsync(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null)
                throw new ArgumentException("Invalid file");

            var extension = contentType?.ToLowerInvariant() switch
            {
                "image/png" => ".png",
                "image/bmp" => ".bmp",
                _ => throw new ArgumentException("Unsupported content type. Use only PNG or BMP.")
            };

            var finalName = $"{fileName}{extension}";
            var fullPath = Path.Combine(_imagesDirectory, finalName);

            using var outStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(outStream);

            return finalName;
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            var fullPath = Path.Combine(_imagesDirectory, fileName);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                return true;
            }
            
            return false;
        }

        public async Task<byte[]?> GetImageAsync(string fileName)
        {
            var fullPath = Path.Combine(_imagesDirectory, fileName);
            
            if (File.Exists(fullPath))
            {
                return await File.ReadAllBytesAsync(fullPath);
            }
            
            return null;
        }
    }
}
