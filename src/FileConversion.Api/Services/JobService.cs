using FileConversion.Api.Interfaces;
using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;

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
    }
}
