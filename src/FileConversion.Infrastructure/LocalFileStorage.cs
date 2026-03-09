using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace FileConversion.Infrastructure
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly StorageSettings _storageSettings;

        public LocalFileStorage(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings.Value;
        }

        public Task DeleteFileAsync(string filePath)
        {
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        public Task<Stream> GetFileAsync(string filePath)
        {
            var stream = File.OpenRead(filePath) as Stream;

            return Task.FromResult(stream);
        }

        public async Task<string> SaveFileAsync(Stream stream, string fileName)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var fileId = Guid.NewGuid();

            var uploadsDirectory = _storageSettings.BasePath;

            var path = Path.Combine(uploadsDirectory, $"{fileId}_{fileName}");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            return path;
        }
    }
}
