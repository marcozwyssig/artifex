using Microsoft.Extensions.Options;
using Artifex.DeviceManagement.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Node Agent API", Version = "v1" });
});

// Configuration
builder.Services.Configure<DiscoveryOptions>(
    builder.Configuration.GetSection(DiscoveryOptions.SectionName));

// HTTP Clients
builder.Services.AddHttpClient<DeviceManagementClient>((serviceProvider, client) =>
{
    var nodeAgentConfig = builder.Configuration.GetSection("NodeAgent");
    var serviceUrl = nodeAgentConfig["DeviceManagementServiceUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(serviceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Discovery Services
builder.Services.AddSingleton<LocalNetworkDiscoveryService>();

// Background Services
builder.Services.AddHostedService<DiscoveryBackgroundService>();

// Logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var discoveryOptions = app.Services.GetRequiredService<IOptions<DiscoveryOptions>>().Value;

logger.LogInformation("Node Agent starting...");
logger.LogInformation("Device Management Service: {ServiceUrl}",
    builder.Configuration.GetSection("NodeAgent")["DeviceManagementServiceUrl"]);
logger.LogInformation("Configured {Count} network segments for discovery",
    discoveryOptions.NetworkSegments.Count);

foreach (var segment in discoveryOptions.NetworkSegments)
{
    logger.LogInformation("  - {Name}: {Cidr} (Interval: {Interval}min, Auto: {Auto})",
        segment.Name,
        segment.NetworkCidr,
        segment.DiscoveryIntervalMinutes,
        segment.EnableAutoDiscovery);
}

app.Run();
