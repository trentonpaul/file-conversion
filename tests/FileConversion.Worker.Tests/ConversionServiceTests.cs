using System;
using System.IO;
using System.Threading.Tasks;
using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using FileConversion.Worker.Interfaces;
using FileConversion.Worker.Services;
using Moq;
using Xunit;

namespace FileConversion.Worker.Tests
{
    public class ConversionServiceTests
    {
        private readonly Mock<IFileStorage> _fileStorage;
        private readonly Mock<IImageConverter> _imageConverter;
        private readonly ConversionService _service;

        public ConversionServiceTests()
        {
            _fileStorage = new Mock<IFileStorage>();
            _imageConverter = new Mock<IImageConverter>();
            _service = new ConversionService(_fileStorage.Object, _imageConverter.Object);
        }

        [Fact]
        public async Task ConvertAsync_SuccessfulConversion_ReturnsOutputFilePath()
        {
            var job = new ConversionJob("test.png", "input/path", "jpg");
            var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var outputStream = new MemoryStream(new byte[] { 4, 5, 6 });
            var expectedOutputPath = "output/path/test.jpg";

            _fileStorage.Setup(x => x.GetFileAsync(job.UploadFilePath)).ReturnsAsync(inputStream);
            _imageConverter.Setup(x => x.ConvertAsync(inputStream, job.TargetFormat)).ReturnsAsync(outputStream);
            _fileStorage.Setup(x => x.SaveFileAsync(outputStream, "test.jpg")).ReturnsAsync(expectedOutputPath);

            var result = await _service.ConvertAsync(job);

            Assert.Equal(expectedOutputPath, result);
        }

        [Fact]
        public async Task ConvertAsync_FileNotFound_ThrowsException()
        {
            var job = new ConversionJob("test.png", "input/path", "jpg");
            _fileStorage.Setup(x => x.GetFileAsync(job.UploadFilePath)).ReturnsAsync((Stream)null);

            await Assert.ThrowsAsync<Exception>(() => _service.ConvertAsync(job));
        }

        [Fact]
        public async Task ConvertAsync_ConversionFails_ThrowsException()
        {
            var job = new ConversionJob("test.png", "input/path", "jpg");
            var inputStream = new MemoryStream(new byte[] { 1, 2, 3 });
            _fileStorage.Setup(x => x.GetFileAsync(job.UploadFilePath)).ReturnsAsync(inputStream);
            _imageConverter.Setup(x => x.ConvertAsync(inputStream, job.TargetFormat)).ReturnsAsync((Stream)null);

            await Assert.ThrowsAsync<Exception>(() => _service.ConvertAsync(job));
        }
    }
}
