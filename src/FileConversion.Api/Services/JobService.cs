using FileConversion.Api.Interfaces;
using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using System.Collections.Generic;

namespace FileConversion.Api.Services
{
    public class JobService : IJobService
    {
        private readonly ILogger<JobService> _logger;
        private readonly IFileStorage _fileStorage;
        private readonly IJobRepository _jobRepository;
        private readonly IMessagePublisher _messagePublisher;

        public JobService(ILogger<JobService> logger, IFileStorage fileStorage, IJobRepository jobRepository, IMessagePublisher messagePublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        }

        public async Task<ConversionJob> SubmitJobAsync(Stream file, string fileName, string targetFormat)
        {
            var filePath = await _fileStorage.SaveFileAsync(file, fileName);
            var conversionJob = new ConversionJob(fileName, filePath, targetFormat);
            _logger.LogInformation($"File with name {fileName} stored successfully at {filePath}");
            await _jobRepository.CreateJobAsync(conversionJob);
            await _messagePublisher.PublishAsync(conversionJob.Id);

            return conversionJob;
        }
        public async Task<JobStatus?> GetJobStatusAsync(Guid jobId)
        {
            var job = await _jobRepository.GetJobAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning($"Job with ID {jobId} not found.");
                return null;
            }
            return job.Status;
        }

        public async Task<JobResultInfo> GetJobResultsAsync(Guid jobId)
        {
            var job = await _jobRepository.GetJobAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning($"Job with ID {jobId} not found.");
                return new JobResultInfo { Exists = false };
            }
            if (job.Status == JobStatus.Failed)
            {
                _logger.LogWarning($"Job with ID {jobId} failed. Error: {job.ErrorMessage}");
                return new JobResultInfo { Exists = true, Status = job.Status, ErrorMessage = job.ErrorMessage };
            }
            if (job.Status != JobStatus.Complete)
            {
                _logger.LogWarning($"Job with ID {jobId} is not completed. Current status: {job.Status}");
                return new JobResultInfo { Exists = true, Status = job.Status };
            }
            if (job.OutputFilePath == null)
            {
                _logger.LogWarning($"No output file for {jobId} found.");
                return new JobResultInfo { Exists = true, Status = job.Status };
            }
            var resultStream = await _fileStorage.GetFileAsync(job.OutputFilePath!);
            var resultFileName = Path.ChangeExtension(job.OriginalFileName, job.TargetFormat);
            return new JobResultInfo
            {
                Exists = true,
                Status = job.Status,
                FileStream = resultStream,
                FileName = resultFileName
            };
        }

        public async Task<IReadOnlyList<JobStatus?>> GetJobStatusesAsync(IEnumerable<Guid> jobIds)
        {
            var jobs = await _jobRepository.GetJobsAsync(jobIds);
            return jobs.Select(j => j?.Status).ToList();
        }

        public async Task<IReadOnlyList<JobResultInfo>> GetJobResultsAsync(IEnumerable<Guid> jobIds)
        {
            var jobs = await _jobRepository.GetJobsAsync(jobIds);
            var results = new List<JobResultInfo>();
            foreach (var job in jobs)
            {
                if (job == null)
                {
                    results.Add(new JobResultInfo { Exists = false });
                    continue;
                }
                if (job.Status == JobStatus.Failed)
                {
                    results.Add(new JobResultInfo { Exists = true, Status = job.Status, ErrorMessage = job.ErrorMessage });
                    continue;
                }
                if (job.Status != JobStatus.Complete)
                {
                    results.Add(new JobResultInfo { Exists = true, Status = job.Status });
                    continue;
                }
                if (job.OutputFilePath == null)
                {
                    results.Add(new JobResultInfo { Exists = true, Status = job.Status });
                    continue;
                }
                var resultStream = await _fileStorage.GetFileAsync(job.OutputFilePath!);
                var resultFileName = Path.ChangeExtension(job.OriginalFileName, job.TargetFormat);
                results.Add(new JobResultInfo
                {
                    Exists = true,
                    Status = job.Status,
                    FileStream = resultStream,
                    FileName = resultFileName
                });
            }
            return results;
        }
    }
}
