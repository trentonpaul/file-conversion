using FileConversion.Api.Interfaces;
using FileConversion.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, RabbitMqConnectionFactory connectionFactory, IOptions<RabbitMqSettings> options)
    {
        _connectionFactory = connectionFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(Guid jobId)
    {
        var connection = await _connectionFactory.GetConnection();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(jobId);

        _logger.LogInformation($"Publishing to queue {jobId}");

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            body: body
        );
    }
}