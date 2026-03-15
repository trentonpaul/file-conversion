using Castle.Core.Logging;
using FileConversion.Api.Interfaces;
using FileConversion.Api.Services;
using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;
using System.Threading.Tasks;

namespace FileConversion.Api.Tests
{
    public class JobServiceTests
    {
        private readonly JobService _service;
        private readonly Mock<ILogger<JobService>> _logger;
        private readonly Mock<IFileStorage> _fileStorage;
        private readonly Mock<IJobRepository> _jobRepository;
        private readonly Mock<IMessagePublisher> _messagePublisher;

        public JobServiceTests()
        {
            _fileStorage = new Mock<IFileStorage>();
            _jobRepository = new Mock<IJobRepository>();
            _messagePublisher = new Mock<IMessagePublisher>();
            _logger = new Mock<ILogger<JobService>>();
            _service = new JobService(_logger.Object, _fileStorage.Object, _jobRepository.Object, _messagePublisher.Object);
        }

        [Fact]
        public async Task SubmitJobAsync_ShouldReturnJobId()
        {
            // SaveFileAsync returns Task<string> per interface -> return a string (not Guid)
            _fileStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(Guid.NewGuid().ToString());
            _jobRepository.Setup(x => x.CreateJobAsync(It.IsAny<ConversionJob>()))
                .Returns((ConversionJob job) => Task.FromResult(job));
            _messagePublisher.Setup(x => x.PublishAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
            var result = await _service.SubmitJobAsync(stream, "sample.png", "webp");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SubmitJobAsync_ShouldCreateJobWithPendingStatus()
        {
            ConversionJob? capturedJob = null;

            _fileStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(Guid.NewGuid().ToString());
            _jobRepository.Setup(x => x.CreateJobAsync(It.IsAny<ConversionJob>()))
                .Callback<ConversionJob>(job => capturedJob = job)
                .Returns((ConversionJob job) => Task.FromResult(job));
            _messagePublisher.Setup(x => x.PublishAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

            await _service.SubmitJobAsync(stream, "sample.png", "webp");

            Assert.NotNull(capturedJob);
            Assert.Equal(JobStatus.Pending, capturedJob.Status);
        }

        [Fact]
        public async Task SubmitJobAsync_ShouldNotPublish_WhenStorageFails()
        {
            _fileStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Storage unavailable"));

            using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

            await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitJobAsync(stream, "sample.png", "webp"));

            _messagePublisher.Verify(x => x.PublishAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task SubmitJobAsync_ShouldNotSaveJob_WhenPublishFails()
        {
            _fileStorage.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(Guid.NewGuid().ToString());

            _messagePublisher.Setup(x => x.PublishAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("RabbitMQ unavailable"));

            using var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

            await Assert.ThrowsAsync<Exception>(() =>
                _service.SubmitJobAsync(stream, "sample.png", "webp"));

            _jobRepository.Verify(x => x.CreateJobAsync(It.IsAny<ConversionJob>()), Times.Never);
        }

    }
}
