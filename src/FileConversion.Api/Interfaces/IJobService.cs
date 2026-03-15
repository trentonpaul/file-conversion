using FileConversion.Shared.Models;
using System.Collections.Generic;

namespace FileConversion.Api.Interfaces
{
    public class JobResultInfo
    {
        public bool Exists { get; set; }
        public JobStatus? Status { get; set; }
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IJobService
    {
        Task<ConversionJob> SubmitJobAsync(Stream file, string fileName, string targetFormat);

        Task<JobStatus?> GetJobStatusAsync(Guid jobId);
        Task<JobResultInfo> GetJobResultsAsync(Guid jobId);

        // Batch methods
        Task<IReadOnlyList<JobStatus?>> GetJobStatusesAsync(IEnumerable<Guid> jobIds);
        Task<IReadOnlyList<JobResultInfo>> GetJobResultsAsync(IEnumerable<Guid> jobIds);
    }
}
