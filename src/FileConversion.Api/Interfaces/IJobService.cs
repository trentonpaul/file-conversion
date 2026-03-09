using FileConversion.Shared.Models;

namespace FileConversion.Api.Interfaces
{
    public interface IJobService
    {
        Task<ConversionJob> SubmitJobAsync(Stream file, string fileName, string targetFormat);
    }
}
