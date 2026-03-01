namespace FileConversion.Worker.Services
{
    public interface IImageConverter
    {
        Task<Stream> ConvertAsync(Stream input, string to);
    }
}
