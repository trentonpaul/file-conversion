
namespace FileConversion.Shared.Interfaces
{
    public interface IFileStorage
    {
        Task<Guid> SaveFileAsync(Stream fileStream, string fileName);
        Task<Stream> GetFileAsync(Guid fileId);
        Task DeleteFileAsync(Guid fileId);
    }
}