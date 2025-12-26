# Artifex Network Management - Deployment Guide

## Overview

Artifex is a distributed network management system with:
- **Device Management Service** - Central device inventory and discovery coordination
- **Node Agent** - Distributed agents for local network discovery
- **Topology Management** - Network topology mapping
- **Monitoring** - Device health and performance monitoring

## Architecture

```
┌─────────────────────────────────────────────────────┐
│           Device Management Service                 │
│         (Central - Port 5001)                       │
│  - Device Inventory                                 │
│  - Discovery Coordination                           │
│  - SNMP/SSH Communication                           │
│  - Event Bus                                        │
└────────────────┬────────────────────────────────────┘
                 │
                 │ HTTP API
                 │
        ┌────────┴────────┬────────────────┐
        │                 │                │
┌───────▼────────┐ ┌─────▼──────┐ ┌───────▼────────┐
│  Node Agent    │ │ Node Agent │ │  Node Agent    │
│  (HQ)          │ │ (Branch)   │ │  (Data Center) │
│                │ │            │ │                │
│ 192.168.1.0/24 │ │ 10.50.0/24 │ │ 10.0.0.0/24    │
│ 192.168.2.0/24 │ │            │ │ 10.0.1.0/24    │
└────────────────┘ └────────────┘ └────────────────┘
```

## Prerequisites

### Software Requirements
- .NET 8.0 SDK or later
- PostgreSQL 14+ (for Device Management)
- Docker (optional, for containerized deployment)

### Network Requirements
- SNMP v2c access to managed devices
- ICMP (ping) access for discovery
- SSH access for device configuration (optional)

## Deployment Steps

### 1. Deploy Device Management Service

#### Configuration

Edit `src/services/device-management/artifex.device-management.web/cqrs/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DeviceManagementDb": "Host=localhost;Database=artifex_device_management;Username=artifex;Password=<STRONG_PASSWORD>"
  },
  "EventBus": {
    "UseRabbitMQ": false,
    "ExchangeName": "artifex_events"
  }
}
```

#### Database Setup

```bash
# Create database
createdb artifex_device_management

# Run migrations (when implemented)
cd src/services/device-management/artifex.device-management.infrastructure
dotnet ef database update
```

#### Run the Service

```bash
cd src/services/device-management/artifex.device-management.web/api
dotnet run
```

Service will be available at: `http://localhost:5001`

API Documentation: `http://localhost:5001/swagger`

### 2. Deploy Node Agent(s)

#### Configuration

Edit `src/applications/node-agent/artifex.node-agent.web/cqrs/appsettings.json`:

```json
{
  "NodeAgent": {
    "NodeId": "node-agent-hq-001",
    "Location": "Headquarters",
    "DeviceManagementServiceUrl": "http://device-management-service:5001"
  },

  "Discovery": {
    "InitialDelaySeconds": 30,
    "CheckIntervalMinutes": 5,
    "DefaultUsername": "admin",
    "DefaultPassword": "<DEVICE_PASSWORD>",

    "NetworkSegments": [
      {
        "Name": "HQ LAN",
        "NetworkCidr": "192.168.1.0/24",
        "Description": "Headquarters LAN",
        "EnableAutoDiscovery": true,
        "DiscoveryIntervalMinutes": 60
      },
      {
        "Name": "HQ Management",
        "NetworkCidr": "10.0.0.0/24",
        "Description": "Management network",
        "EnableAutoDiscovery": true,
        "DiscoveryIntervalMinutes": 30
      }
    ]
  }
}
```

#### Run the Agent

```bash
cd src/applications/node-agent/artifex.node-agent.web/api
dotnet run
```

Agent will be available at: `http://localhost:5002`

API Documentation: `http://localhost:5002/swagger`

### 3. Multi-Site Deployment

For branch offices or remote locations, deploy additional node agents:

#### Branch Office Agent

```json
{
  "NodeAgent": {
    "NodeId": "node-agent-branch-001",
    "Location": "Branch Office - Chicago",
    "DeviceManagementServiceUrl": "https://artifex.company.com"
  },

  "Discovery": {
    "NetworkSegments": [
      {
        "Name": "Branch LAN",
        "NetworkCidr": "192.168.50.0/24",
        "EnableAutoDiscovery": true,
        "DiscoveryIntervalMinutes": 120
      }
    ]
  }
}
```

#### Data Center Agent

```json
{
  "NodeAgent": {
    "NodeId": "node-agent-dc-001",
    "Location": "Primary Data Center",
    "DeviceManagementServiceUrl": "http://localhost:5001"
  },

  "Discovery": {
    "NetworkSegments": [
      {
        "Name": "DC Management",
        "NetworkCidr": "10.0.0.0/24",
        "DiscoveryIntervalMinutes": 15
      },
      {
        "Name": "DC Storage",
        "NetworkCidr": "10.0.1.0/24",
        "DiscoveryIntervalMinutes": 30
      },
      {
        "Name": "DC Backbone",
        "NetworkCidr": "10.0.2.0/24",
        "DiscoveryIntervalMinutes": 30
      }
    ]
  }
}
```

## Usage

### 1. Automatic Discovery

Node agents will automatically discover devices based on configured schedules.

**Monitor Discovery:**
```bash
# Check node agent logs
curl http://localhost:5002/cqrs/discovery/segments

# View last discovery results
tail -f /var/log/artifex/node-agent.log
```

### 2. Manual Discovery

**Discover All Segments:**
```bash
curl -X POST http://localhost:5002/cqrs/discovery/discover-all
```

Response:
```json
{
  "segmentsScanned": 2,
  "totalDevicesFound": 24,
  "segmentResults": [
    {
      "segmentName": "HQ LAN",
      "devicesFound": 18
    },
    {
      "segmentName": "HQ Management",
      "devicesFound": 6
    }
  ]
}
```

**Discover Specific Segment:**
```bash
curl -X POST http://localhost:5002/cqrs/discovery/discover-segment \
  -H "Content-Type: application/json" \
  -d '{"segmentName": "HQ LAN"}'
```

**Identify Single Device:**
```bash
curl http://localhost:5002/cqrs/discovery/identify/192.168.1.100
```

Response:
```json
{
  "ipAddress": "192.168.1.100",
  "macAddress": "00:1A:2B:3C:4D:5E",
  "hostname": "core-switch-01",
  "vendor": "Cisco",
  "deviceType": "Switch",
  "model": "Catalyst 2960",
  "isReachable": true
}
```

### 3. View Discovered Devices

```bash
# Get all devices
curl http://localhost:5001/cqrs/devices

# Get specific device
curl http://localhost:5001/cqrs/devices/{deviceId}

# Filter by status
curl "http://localhost:5001/cqrs/devices?status=Online&pageSize=50"
```

### 4. Update Device Status

```bash
curl -X PATCH http://localhost:5001/cqrs/devices/{deviceId}/status \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Maintenance",
    "reason": "Scheduled maintenance"
  }'
```

## Docker Deployment

### Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:14
    environment:
      POSTGRES_DB: artifex_device_management
      POSTGRES_USER: artifex
      POSTGRES_PASSWORD: artifex_secure_password
    volumes:
      - postgres-data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  device-management:
    build:
      context: .
      dockerfile: src/services/device-management/Dockerfile
    environment:
      ConnectionStrings__DeviceManagementDb: "Host=postgres;Database=artifex_device_management;Username=artifex;Password=artifex_secure_password"
    ports:
      - "5001:80"
    depends_on:
      - postgres

  node-agent-hq:
    build:
      context: .
      dockerfile: src/applications/node-agent/Dockerfile
    environment:
      NodeAgent__NodeId: "node-agent-hq-001"
      NodeAgent__Location: "Headquarters"
      NodeAgent__DeviceManagementServiceUrl: "http://device-management"
      Discovery__NetworkSegments__0__Name: "HQ LAN"
      Discovery__NetworkSegments__0__NetworkCidr: "192.168.1.0/24"
    network_mode: "host"  # Required for network discovery
    depends_on:
      - device-management

volumes:
  postgres-data:
```

Run:
```bash
docker-compose up -d
```

## Security Considerations

### 1. Credentials Management

**Use Environment Variables:**
```bash
export ARTIFEX_DEFAULT_USERNAME="admin"
export ARTIFEX_DEFAULT_PASSWORD="secure_password"
```

**Or use secrets management:**
- Azure Key Vault
- HashiCorp Vault
- AWS Secrets Manager

### 2. Network Security

- Run node agents in trusted networks
- Use HTTPS for production deployments
- Implement authentication/authorization
- Restrict SNMP community strings

### 3. SNMP Security

- Use SNMP v3 with encryption (future enhancement)
- Restrict SNMP access by source IP
- Use read-only community strings
- Rotate community strings regularly

## Monitoring & Troubleshooting

### Health Checks

**Device Management:**
```bash
curl http://localhost:5001/health
```

**Node Agent:**
```bash
curl http://localhost:5002/health
```

### Logs

**View Application Logs:**
```bash
# Device Management
tail -f /var/log/artifex/device-management.log

# Node Agent
tail -f /var/log/artifex/node-agent.log
```

**Enable Debug Logging:**

Edit `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Artifex": "Trace"
    }
  }
}
```

### Common Issues

**1. Discovery finds no devices**
- Check network connectivity
- Verify CIDR notation is correct
- Ensure ICMP (ping) is not blocked
- Check firewall rules

**2. SNMP identification fails**
- Verify SNMP community string
- Check SNMP is enabled on devices
- Ensure UDP port 161 is open
- Test with snmpwalk: `snmpwalk -v2c -c public <ip> system`

**3. Node agent cannot reach Device Management**
- Check DeviceManagementServiceUrl configuration
- Verify network routing
- Check DNS resolution
- Test connectivity: `curl http://device-management:5001/health`

## Performance Tuning

### Discovery Intervals

**High Priority Networks** (Critical infrastructure):
```json
"DiscoveryIntervalMinutes": 15
```

**Normal Priority Networks**:
```json
"DiscoveryIntervalMinutes": 60
```

**Low Priority Networks** (Guest WiFi, IoT):
```json
"DiscoveryIntervalMinutes": 240
```

### Network Scan Size

Default limit: 1024 hosts per subnet

For larger networks, break into smaller subnets:
```json
"NetworkSegments": [
  {"Name": "Corp A", "NetworkCidr": "10.0.0.0/25"},
  {"Name": "Corp B", "NetworkCidr": "10.0.0.128/25"}
]
```

## Backup & Recovery

### Database Backup

```bash
pg_dump artifex_device_management > backup_$(date +%Y%m%d).sql
```

### Configuration Backup

```bash
tar -czf artifex_config_$(date +%Y%m%d).tar.gz \
  src/services/device-management/artifex.device-management.web/cqrs/appsettings.json \
  src/applications/node-agent/artifex.node-agent.web/cqrs/appsettings.json
```

## Next Steps

1. **Enable RabbitMQ** for event-driven architecture
2. **Add Authentication** using OAuth2/OpenID Connect
3. **Implement SNMP v3** for enhanced security
4. **Add Web UI** for device management
5. **Configure Monitoring** using Prometheus/Grafana
6. **Set up Alerting** for device status changes

## Support

For issues and questions:
- GitHub Issues: https://github.com/your-org/artifex/issues
- Documentation: https://docs.artifex.company.com
