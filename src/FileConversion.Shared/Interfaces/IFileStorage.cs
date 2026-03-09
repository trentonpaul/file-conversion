
namespace FileConversion.Shared.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(Stream stream, string fileName);
        Task<Stream> GetFileAsync(string filePath);
        Task DeleteFileAsync(string filePath);
    }
}