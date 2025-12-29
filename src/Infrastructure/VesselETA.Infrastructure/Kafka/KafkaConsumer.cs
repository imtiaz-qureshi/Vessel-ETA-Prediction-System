using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace VesselETA.Infrastructure.Kafka;

public interface IKafkaConsumer<T>
{
    Task StartConsumingAsync(string topic, Func<T, Task> messageHandler, CancellationToken cancellationToken);
}

public class KafkaConsumer<T> : IKafkaConsumer<T>, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer<T>> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaConsumer(IOptions<KafkaConfiguration> config, ILogger<KafkaConsumer<T>> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config.Value.BootstrapServers,
            GroupId = config.Value.GroupId,
            AutoOffsetReset = config.Value.AutoOffsetReset,
            EnableAutoCommit = config.Value.EnableAutoCommit,
            SessionTimeoutMs = config.Value.SessionTimeoutMs,
            MaxPollIntervalMs = config.Value.MaxPollIntervalMs
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
            .Build();
    }

    public async Task StartConsumingAsync(string topic, Func<T, Task> messageHandler, CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);
        _logger.LogInformation("Started consuming from topic: {Topic}", topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    if (consumeResult?.Message?.Value != null)
                    {
                        var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value, _jsonOptions);
                        if (message != null)
                        {
                            await messageHandler(message);
                            _consumer.Commit(consumeResult);
                        }
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from topic {Topic}", topic);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing message from topic {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing message from topic {Topic}", topic);
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    public void Dispose()
    {
        _consumer?.Dispose();
    }
}