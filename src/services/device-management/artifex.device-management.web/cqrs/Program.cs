using Artifex.DeviceManagement.Application.Commands;
using Artifex.DeviceManagement.Application.Queries;
using Artifex.DeviceManagement.Domain.Repositories;
using Artifex.DeviceManagement.Domain.Services;
using Artifex.DeviceManagement.Domain.Factories;
using Artifex.DeviceManagement.Infrastructure.Communication;
using Artifex.DeviceManagement.Infrastructure.Communication.Snmp;
using Artifex.DeviceManagement.Infrastructure.Communication.Acl;
using Artifex.DeviceManagement.Infrastructure.Persistence;
using Artifex.DeviceManagement.Infrastructure.Persistence.Repositories;
using Artifex.DeviceManagement.Infrastructure.Messaging.Consumers;
using Artifex.Shared.Infrastructure.Commands;
using Artifex.Shared.Cqrs.Queries;
using Artifex.Shared.Domain;
using Artifex.Shared.Infrastructure.Configuration;
using Artifex.Shared.Infrastructure.Messaging;
using Artifex.Shared.Infrastructure.Persistence;
using Artifex.Shared.Ui.Configuration;
using Microsoft.EntityFrameworkCore;
using Consul;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Device Management API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<DeviceManagementDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DeviceManagementDb"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("artifex.device-management.infrastructure")));

// Repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

// Factories
builder.Services.AddScoped<DeviceFactory>();

// Discovery Services
builder.Services.AddScoped<NetworkScanner>();
builder.Services.AddScoped<SnmpDeviceIdentifier>();
builder.Services.AddScoped<IDeviceDiscoveryService, DeviceDiscoveryService>();

// Anti-Corruption Layer for external integrations
builder.Services.AddScoped<ISnmpAdapter, SnmpAdapter>();

// Messaging (abstracted over MassTransit/InMemory)
// Uses IMessageBus abstraction - switchable via configuration
builder.Services.AddMessaging(builder.Configuration, messaging =>
{
    // Register consumers - works with both MassTransit and InMemory implementations
    messaging.AddConsumer<DeviceRegisteredConsumer>();
    messaging.AddConsumer<DeviceStatusChangedConsumer>();
});

// Shared infrastructure (Logging, Unit of Work)
builder.Services.AddLoggerAdapter();
builder.Services.AddUnitOfWork<DeviceManagementDbContext>();

// Consul for service discovery
builder.Services.AddSingleton<IConsulClient, ConsulClient>(sp =>
    new ConsulClient(config =>
    {
        config.Address = new Uri(builder.Configuration.GetValue<string>("Consul:Address") ?? "http://localhost:8500");
    }));

// HTTP clients with Polly resilience policies
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // Log retry attempts
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}

// Example: Configure HTTP client with Polly (add your actual HTTP clients here)
// builder.Services.AddHttpClient<IDeviceManagementClient, DeviceManagementClient>()
//     .AddPolicyHandler(GetRetryPolicy())
//     .AddPolicyHandler(GetCircuitBreakerPolicy());

// Command Handlers
builder.Services.AddScoped<ICommandHandler<RegisterDeviceCommand, Result<Guid>>, RegisterDeviceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateDeviceStatusCommand, Result>, UpdateDeviceStatusCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteDeviceCommand, Result>, DeleteDeviceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DiscoverDevicesCommand, Result<DeviceDiscoveryResult>>, DiscoverDevicesCommandHandler>();

// Query Handlers
builder.Services.AddScoped<IQueryHandler<GetDeviceByIdQuery, Result<DeviceDto>>, GetDeviceByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetAllDevicesQuery, PagedResult<DeviceDto>>, GetAllDevicesQueryHandler>();
builder.Services.AddScoped<IQueryHandler<IdentifyDeviceQuery, Result<DiscoveredDevice>>, IdentifyDeviceQueryHandler>();

var app = builder.Build();

// Register with Consul
if (!app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Consul:Enabled"))
{
    var consulClient = app.Services.GetRequiredService<IConsulClient>();
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

    var serviceId = $"device-management-{Guid.NewGuid()}";
    var serviceAddress = builder.Configuration.GetValue<string>("Consul:ServiceAddress") ?? "localhost";
    var servicePort = builder.Configuration.GetValue<int>("Consul:ServicePort", 5000);

    var registration = new AgentServiceRegistration
    {
        ID = serviceId,
        Name = "device-management",
        Address = serviceAddress,
        Port = servicePort,
        Check = new AgentServiceCheck
        {
            HTTP = $"http://{serviceAddress}:{servicePort}/health",
            Interval = TimeSpan.FromSeconds(10),
            Timeout = TimeSpan.FromSeconds(5),
            DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
        }
    };

    lifetime.ApplicationStarted.Register(async () =>
    {
        await consulClient.Agent.ServiceRegister(registration);
    });

    lifetime.ApplicationStopping.Register(async () =>
    {
        await consulClient.Agent.ServiceDeregister(serviceId);
    });
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSharedUi(); // Adds logging and exception handling middleware

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint for Consul
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
