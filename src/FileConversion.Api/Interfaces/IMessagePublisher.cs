using FileConversion.Shared.Models;

namespace FileConversion.Api.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(Guid jobId);
    }
}
