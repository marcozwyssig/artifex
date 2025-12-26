# Messaging Abstraction Layer

**Purpose**: Abstract messaging infrastructure (MassTransit, InMemory) behind interfaces following Dependency Inversion Principle

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│              Application Layer (Handlers)                │
│                                                          │
│     Depends on: IMessageBus (abstraction)                │
└────────────────────────┬────────────────────────────────┘
                         │
          ┌──────────────▼──────────────┐
          │    IMessageBus Interface    │
          │    (Shared.Infrastructure)  │
          └──────────────┬──────────────┘
                         │
         ┌───────────────┴────────────────┐
         │                                │
┌────────▼────────┐           ┌──────────▼──────────┐
│ MassTransit     │           │  InMemory           │
│ Implementation  │           │  Implementation     │
│  (Production)   │           │  (Development)      │
└─────────────────┘           └─────────────────────┘
```

## Benefits of Abstraction

✅ **Dependency Inversion** - Domain/Application depends on abstraction, not concrete implementation
✅ **Switchable Implementations** - Toggle between MassTransit and InMemory via configuration
✅ **Testability** - Easy to mock IMessageBus for unit tests
✅ **Future-Proof** - Can swap to different messaging library without changing application code
✅ **Development Speed** - InMemory implementation for fast local development without RabbitMQ

---

## Core Interfaces

### IMessageBus

```csharp
/// <summary>
/// Abstraction over message bus implementation
/// </summary>
public interface IMessageBus
{
    // Publish event to all subscribers
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    // Publish multiple events
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    // Send command to specific consumer
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class;

    // Send command to specific endpoint
    Task SendAsync<TCommand>(Uri destinationAddress, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class;
}
```

### IMessageConsumer<TMessage>

```csharp
/// <summary>
/// Abstraction for message consumers
/// </summary>
public interface IMessageConsumer<in TMessage>
    where TMessage : class
{
    Task ConsumeAsync(TMessage message, CancellationToken cancellationToken = default);
}
```

---

## Implementations

### 1. MassTransitMessageBus (Production)

**When to Use**: Production, staging, integration testing
**Requirements**: RabbitMQ server running
**Features**:
- Automatic retry with exponential backoff
- Dead letter queue for failed messages
- Durable queues (survives broker restart)
- Distributed messaging across services
- Message persistence

**Configuration**:
```json
{
  "Messaging": {
    "UseInMemory": false
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "artifex",
    "Password": "artifex_dev_password",
    "VirtualHost": "/"
  }
}
```

### 2. InMemoryMessageBus (Development)

**When to Use**: Local development, unit tests
**Requirements**: None (no external dependencies)
**Features**:
- Synchronous message handling (same process)
- Fast feedback loop
- No infrastructure setup required
- Perfect for unit testing

**Configuration**:
```json
{
  "Messaging": {
    "UseInMemory": true
  }
}
```

---

## Usage Examples

### Publishing Events (in Command Handlers)

```csharp
public class RegisterDeviceCommandHandler : ICommandHandler<RegisterDeviceCommand, Result<Guid>>
{
    private readonly IMessageBus _messageBus;

    public RegisterDeviceCommandHandler(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task<Result<Guid>> HandleAsync(
        RegisterDeviceCommand command,
        CancellationToken cancellationToken)
    {
        // ... create device ...

        // Publish events via abstraction
        foreach (var domainEvent in device.DomainEvents)
        {
            await _messageBus.PublishAsync(domainEvent, cancellationToken);
        }

        return Result.Success(device.Id);
    }
}
```

### Consuming Messages

Consumers implement **both** interfaces for compatibility:

```csharp
public class DeviceRegisteredConsumer :
    IConsumer<DeviceRegisteredEvent>,           // MassTransit interface
    IMessageConsumer<DeviceRegisteredEvent>     // Our abstraction
{
    // MassTransit interface implementation
    public Task Consume(ConsumeContext<DeviceRegisteredEvent> context)
    {
        return ConsumeAsync(context.Message, context.CancellationToken);
    }

    // Our abstraction interface implementation
    public async Task ConsumeAsync(
        DeviceRegisteredEvent message,
        CancellationToken cancellationToken = default)
    {
        // Handle the event
        _logger.LogInformation("Device {DeviceId} registered", message.DeviceId);
    }
}
```

### Registering Consumers (in Program.cs)

```csharp
// Automatically chooses MassTransit or InMemory based on configuration
builder.Services.AddMessaging(builder.Configuration, messaging =>
{
    messaging.AddConsumer<DeviceRegisteredConsumer>();
    messaging.AddConsumer<DeviceStatusChangedConsumer>();
});
```

---

## Configuration-Based Switching

### Development (InMemory)

**appsettings.Development.json**:
```json
{
  "Messaging": {
    "UseInMemory": true
  }
}
```

**Behavior**:
- No RabbitMQ required
- Messages handled synchronously
- Fast startup
- Perfect for TDD

### Production (MassTransit)

**appsettings.Production.json**:
```json
{
  "Messaging": {
    "UseInMemory": false
  },
  "RabbitMQ": {
    "Host": "rabbitmq-cluster.production.internal",
    "Port": 5672,
    "Username": "artifex_prod",
    "Password": "${RABBITMQ_PASSWORD}",
    "VirtualHost": "/artifex"
  }
}
```

**Behavior**:
- Uses RabbitMQ
- Distributed messaging
- Automatic retry and DLQ
- Message persistence

---

## Dependency Injection Flow

```
1. Program.cs calls AddMessaging()
   ↓
2. Reads configuration: Messaging:UseInMemory
   ↓
3a. IF UseInMemory == true:
    → Calls AddInMemoryMessaging()
    → Registers InMemoryMessageBus as IMessageBus
    → Registers consumers in DI
    → Subscribes consumers to message types

3b. IF UseInMemory == false:
    → Calls AddMassTransitMessaging()
    → Registers MassTransit services
    → Registers MassTransitMessageBus as IMessageBus
    → Configures RabbitMQ connection
    → Registers consumers with MassTransit

4. Application code depends on IMessageBus
   → DI container injects appropriate implementation
```

---

## Testing with InMemory Bus

### Unit Test Example

```csharp
[Fact]
public async Task RegisterDevice_PublishesEvent_WhenSuccessful()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddInMemoryMessaging(messaging =>
    {
        messaging.AddConsumer<DeviceRegisteredConsumer>();
    });

    var sp = services.BuildServiceProvider();
    var messageBus = sp.GetRequiredService<IMessageBus>();

    // Act
    var @event = new DeviceRegisteredEvent(Guid.NewGuid(), "test-device", "192.168.1.1", "Switch");
    await messageBus.PublishAsync(@event);

    // Assert
    // Consumer executed synchronously in same process
}
```

---

## Comparison: Before vs After

### Before (Direct MassTransit Dependency)

```csharp
// ❌ Tightly coupled to MassTransit
public class RegisterDeviceCommandHandler
{
    private readonly IPublishEndpoint _publishEndpoint;  // MassTransit specific

    public RegisterDeviceCommandHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
}

// ❌ Requires RabbitMQ for all environments
// ❌ Hard to test without infrastructure
// ❌ Cannot easily swap messaging library
```

### After (Abstracted)

```csharp
// ✅ Depends on abstraction
public class RegisterDeviceCommandHandler
{
    private readonly IMessageBus _messageBus;  // Abstraction

    public RegisterDeviceCommandHandler(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
}

// ✅ Can use InMemory for development
// ✅ Easy to test with mocks
// ✅ Can swap messaging library without changing handler
```

---

## Migration Path

### Step 1: Update Handlers

Replace `IPublishEndpoint` with `IMessageBus`:

```csharp
// Before
private readonly IPublishEndpoint _publishEndpoint;
await _publishEndpoint.Publish(@event, ct);

// After
private readonly IMessageBus _messageBus;
await _messageBus.PublishAsync(@event, ct);
```

### Step 2: Update Consumers

Implement both interfaces:

```csharp
public class YourConsumer :
    IConsumer<YourEvent>,           // Keep for MassTransit
    IMessageConsumer<YourEvent>     // Add for abstraction
{
    public Task Consume(ConsumeContext<YourEvent> context)
        => ConsumeAsync(context.Message, context.CancellationToken);

    public async Task ConsumeAsync(YourEvent message, CancellationToken ct)
    {
        // Your logic here
    }
}
```

### Step 3: Update Registration

Replace direct MassTransit registration:

```csharp
// Before
builder.Services.AddMassTransit(x => { ... });

// After
builder.Services.AddMessaging(builder.Configuration, messaging =>
{
    messaging.AddConsumer<YourConsumer>();
});
```

---

## Advanced Scenarios

### Custom Message Bus Implementation

Implement `IMessageBus` for custom providers:

```csharp
public class AzureServiceBusMessageBus : IMessageBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : DomainEvent
    {
        // Azure Service Bus implementation
    }
}

// Register in DI
builder.Services.AddScoped<IMessageBus, AzureServiceBusMessageBus>();
```

### Mock for Testing

```csharp
var mockMessageBus = new Mock<IMessageBus>();
mockMessageBus
    .Setup(x => x.PublishAsync(It.IsAny<DeviceRegisteredEvent>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(Task.CompletedTask);

var handler = new RegisterDeviceCommandHandler(repo, uow, mockMessageBus.Object);
```

---

## Best Practices

### ✅ DO

- Depend on `IMessageBus` in application layer
- Use InMemory for local development
- Use MassTransit for production
- Implement both `IConsumer<T>` and `IMessageConsumer<T>` in consumers
- Configure via appsettings.json

### ❌ DON'T

- Don't use `IPublishEndpoint` directly in handlers
- Don't hard-code messaging implementation choice
- Don't skip consumer registration in `AddMessaging()`
- Don't mix InMemory and MassTransit in same environment

---

## Troubleshooting

### Messages not being received (InMemory)

**Problem**: Consumer not registered
**Solution**: Ensure `messaging.AddConsumer<YourConsumer>()` is called

### Messages not being received (MassTransit)

**Problem**: RabbitMQ connection failed
**Solution**: Check RabbitMQ configuration and ensure broker is running

### Consumer called multiple times (InMemory)

**Problem**: Multiple subscriptions
**Solution**: Ensure consumers are registered as Scoped, not Singleton

---

## File Locations

```
src/shared/artifex.shared.infrastructure/messaging/
├── IMessageBus.cs                              # Abstraction interface
├── IMessageConsumer.cs                         # Consumer interface
├── MassTransitMessageBus.cs                    # MassTransit implementation
├── InMemoryMessageBus.cs                       # InMemory implementation
└── MessagingServiceCollectionExtensions.cs     # DI configuration
```

---

## Summary

The messaging abstraction layer provides:

1. **Loose Coupling** - Application doesn't depend on specific messaging library
2. **Flexibility** - Switch between implementations via configuration
3. **Testability** - InMemory implementation for fast tests
4. **Maintainability** - Change messaging library without touching business logic

**Use InMemory for**: Development, unit tests
**Use MassTransit for**: Production, integration tests, staging

---

**Next Steps**:
1. Review appsettings.json configuration
2. Run with InMemory for local development
3. Deploy with MassTransit for production
4. Write tests using InMemory bus
