# Artifex Network Management System - Build Guide

This guide covers building, testing, and running the Artifex Network Management System.

## Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose
- Python 3.9+ with `invoke` (for task automation)
- PostgreSQL 16+ (optional, can use Docker)
- RabbitMQ (optional, can use Docker)

## Quick Start

### 1. Install Dependencies

```bash
# Install Python invoke tool
pip install invoke

# Restore .NET packages
invoke restore
```

### 2. Build the Solution

```bash
# Build entire solution
invoke build

# Or build specific services
invoke build-device-management
invoke build-node-agent
```

### 3. Run with Docker

```bash
# Start all services (recommended for first run)
invoke docker-up

# View logs
invoke docker-logs

# Stop services
invoke docker-down
```

### 4. Run Locally (Development)

```bash
# Setup dev environment (starts only databases)
invoke dev-setup

# Run Device Management service
invoke run-device-management

# In another terminal, run Node Agent
invoke run-node-agent
```

## Available Tasks

Run `invoke --list` to see all available tasks.

### Build Commands

- `invoke clean` - Clean build artifacts
- `invoke restore` - Restore NuGet packages
- `invoke build` - Build entire solution
- `invoke build-all` - Restore and build (recommended)
- `invoke build-device-management` - Build Device Management service only
- `invoke build-node-agent` - Build Node Agent only

### Test Commands

- `invoke test` - Run all tests
- `invoke test-unit` - Run unit tests only
- `invoke test-integration` - Run integration tests only

### Database Commands

- `invoke db-migration-create --name=MigrationName` - Create new migration
- `invoke db-migration-apply` - Apply pending migrations
- `invoke db-migration-rollback --migration-name=MigrationName` - Rollback to specific migration

Example:
```bash
# Create initial migration for device-management
invoke db-migration-create --name=InitialCreate --service=device-management

# Apply migrations
invoke db-migration-apply --service=device-management
```

### Docker Commands

- `invoke docker-build` - Build Docker images
- `invoke docker-up` - Start all services
- `invoke docker-down` - Stop all services
- `invoke docker-logs` - View logs (all services)
- `invoke docker-logs --service=device-management` - View specific service logs
- `invoke docker-restart` - Restart all services
- `invoke docker-restart --service=device-management` - Restart specific service
- `invoke docker-clean` - Remove all containers, volumes, and images

### Run Commands (Local Development)

- `invoke run-device-management` - Run Device Management service
- `invoke run-node-agent` - Run Node Agent

### Development Commands

- `invoke dev-setup` - Setup development environment
- `invoke format` - Format code
- `invoke lint` - Check code formatting

## Project Structure

```
Artifex/
├── Artifex.sln                 # Solution file
├── docker-compose.yml          # Docker composition
├── tasks.py                    # Build automation
├── src/
│   ├── shared/                 # Shared libraries
│   │   ├── artifex.shared.domain/
│   │   ├── artifex.shared.application/
│   │   ├── artifex.shared.infrastructure/
│   │   └── artifex.shared.web/
│   ├── services/               # Microservices
│   │   └── device-management/
│   │       ├── artifex.device-management.domain/
│   │       ├── artifex.device-management.application/
│   │       ├── artifex.device-management.infrastructure/
│   │       └── artifex.device-management.web/
│   └── applications/           # Standalone applications
│       └── node-agent/
│           ├── artifex.node-agent.domain/
│           ├── artifex.node-agent.application/
│           ├── artifex.node-agent.infrastructure/
│           └── artifex.node-agent.web/
└── scripts/
    └── init-db.sql             # Database initialization
```

## Building for Production

### Using Docker (Recommended)

```bash
# Build production images
invoke docker-build

# Run in production mode
docker-compose up -d
```

### Manual Build

```bash
# Build release configuration
dotnet build Artifex.sln -c Release

# Publish Device Management service
dotnet publish src/services/device-management/artifex.device-management.web/cqrs/artifex.device-management.web.csproj \
  -c Release \
  -o ./publish/device-management

# Publish Node Agent
dotnet publish src/applications/node-agent/artifex.node-agent.web/cqrs/artifex.node-agent.web.csproj \
  -c Release \
  -o ./publish/node-agent
```

## Configuration

### Device Management Service

Configuration file: `src/services/device-management/artifex.device-management.web/cqrs/appsettings.json`

Key settings:
- `ConnectionStrings:DeviceManagementDb` - Database connection
- `EventBus:UseRabbitMQ` - Enable/disable RabbitMQ
- `EventBus:ExchangeName` - Event exchange name

### Node Agent

Configuration file: `src/applications/node-agent/artifex.node-agent.web/cqrs/appsettings.json`

Key settings:
- `NodeAgent:NodeId` - Unique agent identifier
- `NodeAgent:DeviceManagementServiceUrl` - Device Management API URL
- `Discovery:NetworkSegments` - Network segments to discover
- `Discovery:DiscoveryIntervalMinutes` - Auto-discovery interval

## API Endpoints

### Device Management Service (Port 5001)

- `POST /cqrs/discovery/discover` - Trigger device discovery
- `GET /cqrs/discovery/identify/{ipAddress}` - Identify single device
- `GET /cqrs/devices` - List all devices
- `GET /cqrs/devices/{id}` - Get device by ID
- `POST /cqrs/devices` - Register new device
- `PUT /cqrs/devices/{id}` - Update device
- `DELETE /cqrs/devices/{id}` - Delete device

Swagger UI: http://localhost:5001/swagger

### Node Agent (Port 5003)

- `POST /cqrs/discovery/discover-all` - Discover all configured segments
- `POST /cqrs/discovery/discover-segment` - Discover specific segment
- `GET /cqrs/discovery/identify/{ipAddress}` - Identify device
- `GET /cqrs/discovery/segments` - List configured segments

Swagger UI: http://localhost:5003/swagger

## Database Management

### Migrations

Entity Framework Core migrations are stored in the Infrastructure layer of each service.

Create migration:
```bash
dotnet ef migrations add MigrationName \
  --project src/services/device-management/artifex.device-management.infrastructure/artifex.device-management.infrastructure.csproj \
  --startup-project src/services/device-management/artifex.device-management.web/cqrs/artifex.device-management.web.csproj
```

Apply migrations:
```bash
dotnet ef database update \
  --project src/services/device-management/artifex.device-management.web/cqrs/artifex.device-management.web.csproj
```

### Using Docker PostgreSQL

```bash
# Start only PostgreSQL
docker-compose up -d postgres

# Connect to database
docker exec -it artifex-postgres psql -U artifex -d artifex_device_management
```

## Troubleshooting

### Build Errors

**Issue**: Missing packages
```bash
# Solution: Restore packages
invoke restore
invoke build
```

**Issue**: Project reference errors
```bash
# Solution: Clean and rebuild
invoke clean
invoke build-all
```

### Docker Issues

**Issue**: Port already in use
```bash
# Solution: Stop existing containers
invoke docker-down
# Or change ports in docker-compose.yml
```

**Issue**: Database connection failed
```bash
# Solution: Wait for database to be ready
docker-compose logs postgres
# Check health status
docker ps
```

### Runtime Errors

**Issue**: Database migrations not applied
```bash
# Solution: Apply migrations manually
invoke db-migration-apply --service=device-management
```

**Issue**: Node Agent can't reach Device Management
```bash
# Solution: Check service URLs in appsettings.json
# In Docker, use service name: http://device-management:5001
# Locally: http://localhost:5001
```

## Network Discovery Notes

### SNMP Requirements

The discovery system uses SNMP v2c. Ensure target devices:
- Have SNMP enabled
- Allow SNMP queries from the node-agent
- Use community string configured in appsettings.json

### Network Scanning

For production deployments:
- Deploy node-agents on-premise at each site
- Configure network segments in appsettings.json
- Use host network mode for Docker deployments
- Ensure node-agents have network access to target devices

Example Docker host network mode:
```yaml
node-agent:
  network_mode: host
  environment:
    - Discovery__NetworkSegments__0__NetworkCidr=192.168.1.0/24
```

## Performance Tuning

### Discovery Performance

Adjust these settings in node-agent appsettings.json:
- `Discovery:CheckIntervalMinutes` - How often to run discovery
- Network segment `DiscoveryIntervalMinutes` - Per-segment interval
- Network segment `EnableAutoDiscovery` - Disable for manual-only discovery

### Database Performance

For large deployments:
- Enable connection pooling (enabled by default)
- Adjust EF Core tracking behavior
- Consider read replicas for queries
- Index frequently queried fields

## Next Steps

1. Review [DEPLOYMENT-GUIDE.md](DEPLOYMENT-GUIDE.md) for production deployment
2. Configure network segments in node-agent
3. Set up monitoring and logging
4. Configure authentication and authorization
5. Set up SSL/TLS certificates

## Support

For issues and questions:
- Check logs: `invoke docker-logs`
- Review configuration files
- Ensure all services are running: `docker ps`
