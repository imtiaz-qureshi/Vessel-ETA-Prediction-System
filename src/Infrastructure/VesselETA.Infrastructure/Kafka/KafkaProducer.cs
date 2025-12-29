using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace VesselETA.Infrastructure.Kafka;

public interface IKafkaProducer<T>
{
    Task PublishAsync(string topic, string key, T message, CancellationToken cancellationToken = default);
}

public class KafkaProducer<T> : IKafkaProducer<T>, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer<T>> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaProducer(IOptions<KafkaConfiguration> config, ILogger<KafkaProducer<T>> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.Value.BootstrapServers,
            Acks = Acks.All,
            RetryBackoffMs = 1000,
            MessageSendMaxRetries = 3,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Error}", e.Reason))
            .Build();
    }

    public async Task PublishAsync(string topic, string key, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedMessage = JsonSerializer.Serialize(message, _jsonOptions);
            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = serializedMessage,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            _logger.LogDebug("Message published to {Topic} at offset {Offset}", topic, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}