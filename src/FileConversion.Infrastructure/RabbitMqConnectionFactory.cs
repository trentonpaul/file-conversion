using FileConversion.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

public class RabbitMqConnectionFactory
{
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;

    public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> options, ILogger<RabbitMqConnectionFactory> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<IConnection> GetConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timespan, attempt, context) =>
                    {
                        _logger.LogInformation($"RabbitMQ connection attempt {attempt} failed ({exception.GetType().Name}). Retrying in {timespan.Seconds}s...");
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password
                };
                _connection = await factory.CreateConnectionAsync();
            });
        }

        return _connection!;
    }
}