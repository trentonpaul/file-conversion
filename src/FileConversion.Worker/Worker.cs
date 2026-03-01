using FileConversion.Worker.Services;

namespace FileConversion.Worker;

public class Worker(ILogger<Worker> logger, IImageConverter converter) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("File conversion worker starting at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: Implement job queue processing logic
            // This could listen to a message queue (RabbitMQ, Azure Service Bus, etc.)
            // or process conversion jobs from a database
            
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker monitoring for conversion jobs at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(5000, stoppingToken);
        }
        
        logger.LogInformation("File conversion worker stopped at: {time}", DateTimeOffset.Now);
    }
}
