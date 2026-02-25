namespace file_conversion_api.Services
{
    public interface IImageConverter
    {
        Task<Stream> ConvertAsync(Stream input, string to);
    }
}