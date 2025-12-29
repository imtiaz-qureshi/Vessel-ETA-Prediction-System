using Serilog;
using VesselETA.AisIngestionSimulator;
using VesselETA.Infrastructure.Kafka;
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

builder.Services.AddSingleton<IKafkaProducer<VesselPosition>, KafkaProducer<VesselPosition>>();
builder.Services.AddHostedService<AisIngestionWorker>();

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