# Device Management Service - Architecture

Clean separation into **API** and **Orchestration** layers that communicate through the **Domain**.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER                            │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  │
│  - Entities & Aggregates                                    │
│  - Domain Services (business logic)                         │
│  - Domain Events (communication protocol)                   │
│  - Repository Interfaces                                    │
└────────────┬──────────────────────────┬─────────────────────┘
             │                          │
             │ depends on               │ depends on
             ▼                          ▼
┌────────────────────────┐   ┌──────────────────────────────┐
│  maestro.device-       │   │  maestro.device-             │
│  management.api        │   │  management.orchestration    │
│                        │   │                              │
│  UI/External Interface │   │  Background Orchestration    │
├────────────────────────┤   ├──────────────────────────────┤
│ - Commands (CQRS)      │   │ - Discovery Workers          │
│ - Queries (CQRS)       │   │ - Monitoring Workers         │
│ - DTOs                 │   │ - Maintenance Workers        │
│ - Controllers          │   │ - Job Configuration          │
│ - API Services         │   │ - Orchestration Logic        │
└────────────────────────┘   └──────────────────────────────┘
             │                          │
             └────────┬─────────────────┘
                      │
                      ▼
         ┌────────────────────────┐
         │  Domain Events Bus     │
         │  (RabbitMQ/MediatR)    │
         └────────────────────────┘
```

## Project Structure

```
device-management/
│
├── maestro.device-management.domain/          # DOMAIN (Core Contract)
│   ├── aggregates/
│   │   └── Device.cs                          # Device aggregate root
│   ├── entities/
│   │   ├── DeviceRole.cs                      # Router/Switch/Server roles
│   │   ├── Port.cs
│   │   ├── Link.cs
│   │   └── LocalNetworkSegment.cs
│   ├── events/
│   │   ├── DeviceRegisteredEvent.cs           # Published by API
│   │   ├── DeviceDiscoveredEvent.cs           # Published by Orchestration
│   │   └── DiscoveryRequestedEvent.cs         # API → Orchestration
│   ├── services/
│   │   └── IDeviceDiscoveryService.cs         # Shared business logic
│   └── repositories/
│       └── IDeviceRepository.cs               # Data access interface
│
├── maestro.device-management.api/             # API LAYER
│   ├── commands/
│   │   ├── devices/
│   │   │   ├── RegisterDeviceCommand.cs
│   │   │   ├── UpdateDeviceCommand.cs
│   │   │   └── DeleteDeviceCommand.cs
│   │   └── discovery/
│   │       └── TriggerDiscoveryCommand.cs     # Manual trigger from UI
│   ├── queries/
│   │   ├── devices/
│   │   │   ├── GetDeviceByIdQuery.cs
│   │   │   └── GetAllDevicesQuery.cs
│   │   └── discovery/
│   │       └── GetDiscoveryStatusQuery.cs
│   ├── dtos/
│   │   ├── DeviceDto.cs
│   │   └── DiscoveredDeviceDto.cs
│   ├── services/
│   │   └── DeviceManagementClient.cs          # HTTP client
│   └── controllers/
│       └── DiscoveryController.cs
│
└── maestro.device-management.orchestration/   # ORCHESTRATION LAYER
    ├── discovery/
    │   ├── NetworkDiscoveryWorker.cs          # Autonomous discovery
    │   └── TopologyDiscoveryWorker.cs         # LLDP/CDP discovery
    ├── monitoring/
    │   ├── DeviceHealthCheckWorker.cs
    │   └── MetricsCollectionWorker.cs
    ├── maintenance/
    │   └── StaleDeviceCleanupWorker.cs
    ├── configuration/
    │   └── DiscoveryOptions.cs
    └── services/
        └── (orchestration-specific services)
```

## Shared Projects

```
shared/
├── maestro.shared.domain/          # Base classes for all domains
├── maestro.shared.application/     # CQRS base classes
├── maestro.shared.api/             # API-specific shared code
└── maestro.shared.orchestration/   # Orchestration-specific shared code
```

## Communication Patterns

### Pattern 1: User Action → API → Domain

```
User clicks "Register Device"
  ↓
API Controller
  ↓
RegisterDeviceCommand
  ↓
RegisterDeviceCommandHandler
  ↓
Domain: Device.Create()
  ↓
Repository.AddAsync()
  ↓
DeviceRegisteredEvent published
  ↓
Event Bus → (Orchestration can listen if needed)
```

### Pattern 2: Scheduled Worker → Domain

```
Timer triggers NetworkDiscoveryWorker
  ↓
Worker uses Domain Service: IDeviceDiscoveryService
  ↓
Domain Service scans network (via Infrastructure)
  ↓
For each discovered device:
  - Domain: Device.CreateFromDiscovery()
  - Repository.AddAsync()
  - DeviceDiscoveredEvent published
  ↓
Event Bus → (API can listen to update UI)
```

### Pattern 3: API → Events → Orchestration

```
User clicks "Discover Now"
  ↓
TriggerDiscoveryCommand
  ↓
CommandHandler publishes DiscoveryRequestedEvent
  ↓
Event Bus
  ↓
NetworkDiscoveryWorker listens to event
  ↓
Worker executes discovery immediately
```

## Key Principles

### ✅ Correct Communication

**Through Domain:**
- ✅ Both use Domain Services for business logic
- ✅ Both use Domain Events for communication
- ✅ Both use Domain Repositories for data access

**Example:**
```csharp
// API uses domain
var device = Device.Create(ipAddress, hostname);
await _deviceRepository.AddAsync(device);
device.AddDomainEvent(new DeviceRegisteredEvent(device.Id));

// Orchestration uses domain
var devices = await _discoveryService.DiscoverAsync(networkCidr);
foreach (var info in devices)
{
    var device = Device.CreateFromDiscovery(info);
    await _deviceRepository.AddAsync(device);
    device.AddDomainEvent(new DeviceDiscoveredEvent(device.Id));
}
```

### ❌ Incorrect Communication

```csharp
// BAD: Orchestration calling API directly
await _apiClient.RegisterDeviceAsync(...); // NO!

// BAD: API calling Orchestration directly
await _discoveryWorker.TriggerDiscovery(); // NO!

// BAD: Shared application services between API and Orchestration
services.AddScoped<ISharedApplicationService, ...>(); // NO!
```

## Dependency Graph

```
maestro.shared.domain
         ▲
         │
maestro.device-management.domain
         ▲
         ├─────────────────────┬─────────────────────┐
         │                     │                     │
maestro.shared.api    maestro.shared.orchestration   │
         ▲                     ▲                     │
         │                     │                     │
maestro.device-        maestro.device-               │
management.api         management.orchestration      │
         │                     │                     │
         └─────────────────────┴─────────────────────┘
                              │
                   maestro.device-management.
                   infrastructure
```

**Key Points:**
- ✅ API and Orchestration are completely independent
- ✅ Both depend on Domain
- ✅ Infrastructure depends on Domain (not shown in graph)
- ✅ No circular dependencies

## Deployment Scenarios

### Scenario 1: Monolith (Development)

```
Single Process:
├── Presentation (Web API)
├── maestro.device-management.api
├── maestro.device-management.orchestration
├── Infrastructure
└── Domain
```

### Scenario 2: Distributed (Production)

```
API Server:                    Orchestration Server:
├── Presentation               ├── orchestration
├── api                        ├── Infrastructure
├── Infrastructure             └── Domain
└── Domain
         │
         └──── Event Bus (RabbitMQ) ────┘
```

### Scenario 3: Node-Agent (On-Premise)

```
Node-Agent:
├── orchestration  ← ONLY orchestration
├── Infrastructure
└── Domain

Registers discovered devices via HTTP to central API
```

## Benefits

✅ **True Separation**: API and Orchestration completely independent
✅ **Domain-Centric**: All communication through domain layer
✅ **Event-Driven**: Loose coupling via domain events
✅ **Clear Naming**: "API" vs "Orchestration" is self-explanatory
✅ **Scalable**: Orchestration can run on separate workers
✅ **Testable**: Each layer tested independently
✅ **DDD Compliant**: Domain at the center, proper hexagonal architecture

## Naming Conventions

- **API**: User-facing, request/response, synchronous
- **Orchestration**: Background workers, autonomous, asynchronous
- **Domain**: Business logic, events, repositories (shared contract)

This is **clean architecture with proper domain-driven design**!
