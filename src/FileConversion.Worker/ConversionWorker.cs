using FileConversion.Infrastructure.Settings;
using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using FileConversion.Worker.Interfaces;
using FileConversion.Worker.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

public class ConversionWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger _logger;

    public ConversionWorker(
        ILogger<ConversionWorker> logger,
        RabbitMqConnectionFactory connectionFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _scopeFactory = scopeFactory;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionFactory.GetConnection();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var conversionService = scope.ServiceProvider.GetRequiredService<IConversionService>();

            var jobId = JsonSerializer.Deserialize<Guid>(args.Body.Span);

            _logger.LogInformation($"Worker converting job {jobId}");

            var job = await jobRepository.GetJobAsync(jobId);

            if (job == null)
                throw new InvalidOperationException($"No job with id '{jobId}' found");


            _logger.LogInformation($"Job {jobId} found in DB. Updating status to pending.");

            job.Status = JobStatus.Pending;
            await jobRepository.UpdateJobAsync(job);

            _logger.LogInformation($"Successfully updated job status to pending. Now converting to {job.TargetFormat}.");

            try
            {
                var outputFilePath = await conversionService.ConvertAsync(job);
                _logger.LogInformation($"Successfully converted file and saved to {job.OutputFilePath}.");
                job.Status = JobStatus.Complete;
                job.OutputFilePath = outputFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to convert file for job id: {job.Id}");
                job.Status = JobStatus.Failed;
            }

            _logger.LogInformation($"Updating job details.");
            await jobRepository.UpdateJobAsync(job);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: _settings.QueueName,
            autoAck: false,
            consumer: consumer
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}