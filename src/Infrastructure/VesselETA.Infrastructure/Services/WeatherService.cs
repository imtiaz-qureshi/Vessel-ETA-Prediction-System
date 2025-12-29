using Microsoft.Extensions.Logging;
using System.Text.Json;
using VesselETA.Domain.Models;

namespace VesselETA.Infrastructure.Services;

public interface IWeatherService
{
    Task<WeatherData> GetWeatherDataAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<WeatherData> GetWeatherDataAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://marine-api.open-meteo.com/v1/marine?latitude={latitude:F4}&longitude={longitude:F4}&current=wind_speed_10m,wind_direction_10m,wave_height&wind_speed_unit=kn";
            
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var weatherResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(response, _jsonOptions);

            if (weatherResponse?.Current == null)
            {
                _logger.LogWarning("No weather data received for coordinates {Lat}, {Lon}", latitude, longitude);
                return CreateDefaultWeatherData(latitude, longitude);
            }

            var windSpeedKnots = weatherResponse.Current.WindSpeed10m ?? 0;
            var waveHeight = weatherResponse.Current.WaveHeight ?? 0;
            var severityFactor = CalculateSeverityFactor(windSpeedKnots, waveHeight);

            return new WeatherData
            {
                Latitude = latitude,
                Longitude = longitude,
                WindSpeedKnots = windSpeedKnots,
                WindDirection = weatherResponse.Current.WindDirection10m ?? 0,
                WaveHeightMeters = waveHeight,
                VisibilityKm = 10, // Default visibility
                Conditions = GetConditionsDescription(windSpeedKnots, waveHeight),
                TimestampUtc = DateTime.UtcNow,
                SeverityFactor = severityFactor
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather data for coordinates {Lat}, {Lon}", latitude, longitude);
            return CreateDefaultWeatherData(latitude, longitude);
        }
    }

    private static WeatherData CreateDefaultWeatherData(double latitude, double longitude)
    {
        return new WeatherData
        {
            Latitude = latitude,
            Longitude = longitude,
            WindSpeedKnots = 10,
            WindDirection = 180,
            WaveHeightMeters = 1.0,
            VisibilityKm = 10,
            Conditions = "Moderate",
            TimestampUtc = DateTime.UtcNow,
            SeverityFactor = 0.3
        };
    }

    private static double CalculateSeverityFactor(double windSpeedKnots, double waveHeightMeters)
    {
        // Simple severity calculation based on wind and wave conditions
        var windFactor = Math.Min(windSpeedKnots / 40.0, 1.0); // 40+ knots = severe
        var waveFactor = Math.Min(waveHeightMeters / 4.0, 1.0); // 4+ meters = severe
        
        return Math.Max(windFactor, waveFactor);
    }

    private static string GetConditionsDescription(double windSpeedKnots, double waveHeightMeters)
    {
        return (windSpeedKnots, waveHeightMeters) switch
        {
            ( < 10, < 1) => "Calm",
            ( < 20, < 2) => "Moderate",
            ( < 30, < 3) => "Rough",
            _ => "Severe"
        };
    }

    private class OpenMeteoResponse
    {
        public CurrentWeather? Current { get; set; }
    }

    private class CurrentWeather
    {
        public double? WindSpeed10m { get; set; }
        public double? WindDirection10m { get; set; }
        public double? WaveHeight { get; set; }
    }
}