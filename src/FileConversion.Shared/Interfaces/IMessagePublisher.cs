using FileConversion.Shared.Models;

namespace FileConversion.Shared.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(ConversionJob job);
    }
}
