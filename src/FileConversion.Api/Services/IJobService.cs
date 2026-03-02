namespace FileConversion.Api.Services
{
    public interface IJobService
    {
        Task<Guid> SubmitJobAsync(Stream file, string fileName, string targetFormat);
    }
}
