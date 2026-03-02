using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;
using Microsoft.Extensions.Options;

namespace FileConversion.Infrastructure
{
    public class LocalFileStorage : IFileStorage
    {

        private readonly StorageSettings _storageSettings;

        public LocalFileStorage(IOptions<StorageSettings> storageSettings)
        {
            _storageSettings = storageSettings.Value;
        }

        public Task DeleteFileAsync(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetFileAsync(Guid fileId)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> SaveFileAsync(Stream stream, string fileName)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var fileId = Guid.NewGuid();

            var uploadsDirectory = Path.Combine(_storageSettings.BasePath, "uploads");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var path = Path.Combine(uploadsDirectory, $"{fileId}_fileName");

            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            return fileId;
        }
    }
}
