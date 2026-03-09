using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using FileConversion.Worker.Interfaces;

namespace FileConversion.Worker.Services
{

    public class ConversionService : IConversionService
    {
        private readonly IImageConverter _imageConverter;
        private readonly IFileStorage _fileStorage;

        public ConversionService(IFileStorage fileStorage, IImageConverter imageConverter)
        {
            _fileStorage = fileStorage;
            _imageConverter = imageConverter;
        }

        public async Task<string> ConvertAsync(ConversionJob job)
        {
            var file = await _fileStorage.GetFileAsync(job.UploadFilePath);

            if (file == null)
            {
                throw new Exception($"Failed to open file with path '{job.UploadFilePath}' (job id: {job.Id}).");
            }

            var outputFile = await _imageConverter.ConvertAsync(file, job.TargetFormat);

            if (outputFile == null)
            {
                throw new Exception($"Failed to convert file with path '{job.UploadFilePath}' (job id: {job.Id}).");
            }

            var outputFileName = Path.GetFileNameWithoutExtension(job.OriginalFileName) + "." + job.TargetFormat;

            var outputFilePath = await _fileStorage.SaveFileAsync(outputFile, outputFileName);

            return outputFilePath;
        }
    }
}
