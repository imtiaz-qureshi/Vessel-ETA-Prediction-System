using Serilog;
using VesselETA.ApiGateway.Hubs;
using VesselETA.ApiGateway.Services;
using VesselETA.Infrastructure.Kafka;
using VesselETA.Infrastructure.Services;
using VesselETA.Domain.Models;

Console.WriteLine("Starting Vessel ETA API Gateway...");

var builder = WebApplication.CreateBuilder(args);

// Configure URLs explicitly
builder.WebHost.UseUrls("http://localhost:5000");

Console.WriteLine("Configuring services...");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Kafka
builder.Services.Configure<KafkaConfiguration>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IKafkaConsumer<EtaPrediction>, KafkaConsumer<EtaPrediction>>();
builder.Services.AddSingleton<IDistanceCalculator, DistanceCalculator>();

// Configure services
builder.Services.AddSingleton<IPortService, PortService>();
builder.Services.AddSingleton<IVesselService, VesselService>();
builder.Services.AddSingleton<IEtaStreamService, EtaStreamService>();

builder.Services.AddHostedService<EtaStreamBackgroundService>();

Console.WriteLine("Building application...");
var app = builder.Build();

Console.WriteLine("Configuring HTTP pipeline...");

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vessel ETA API V1");
    c.RoutePrefix = "swagger";
});

app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<EtaHub>("/hubs/eta");

// Log the URLs where the application is listening
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("=== Vessel ETA API Gateway Started ===");
    Console.WriteLine("- API: http://localhost:5000");
    Console.WriteLine("- Swagger UI: http://localhost:5000/swagger");
    Console.WriteLine("- SignalR Hub: ws://localhost:5000/hubs/eta");
    Console.WriteLine("=====================================");
});

Console.WriteLine("Starting HTTP server on http://localhost:5000...");
app.Run();