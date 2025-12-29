using Confluent.Kafka;

namespace VesselETA.Infrastructure.Kafka;

public class KafkaConfiguration
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "vessel-eta-group";
    public int SessionTimeoutMs { get; set; } = 30000;
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    public bool EnableAutoCommit { get; set; } = false;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public static class KafkaTopics
{
    public const string RawAisPositions = "raw-ais-positions";
    public const string EtaPredictions = "eta-predictions";
    public const string WeatherUpdates = "weather-updates";
}