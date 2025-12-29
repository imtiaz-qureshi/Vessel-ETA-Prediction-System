using Serilog;
using VesselETA.EtaEngine;
using VesselETA.EtaEngine.Services;
using VesselETA.Infrastructure.Kafka;
using VesselETA.Infrastructure.Services;
using VesselETA.Domain.Models;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Configure Kafka
builder.Services.Configure<KafkaConfiguration>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IKafkaConsumer<VesselPosition>, KafkaConsumer<VesselPosition>>();
builder.Services.AddSingleton<IKafkaProducer<EtaPrediction>, KafkaProducer<EtaPrediction>>();

// Configure services
builder.Services.AddSingleton<IDistanceCalculator, DistanceCalculator>();
builder.Services.AddSingleton<IPortRepository, PortRepository>();
builder.Services.AddSingleton<IEtaCalculationService, EtaCalculationService>();
builder.Services.AddSingleton<IDelayRiskAssessment, DelayRiskAssessment>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

builder.Services.AddHostedService<EtaEngineWorker>();

var host = builder.Build();

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}