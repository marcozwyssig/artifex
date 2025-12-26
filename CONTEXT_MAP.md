# Artifex - Domain-Driven Design Context Map

**Version**: 1.0
**Date**: 2025-12-22
**Status**: Active Development

---

## Overview

This document defines the bounded contexts in the Artifex network management system and describes their relationships using DDD Context Mapping patterns.

---

## Bounded Contexts

### 1. Device Management (Core Domain)
**Purpose**: Manages network devices, their registration, discovery, and lifecycle

**Ubiquitous Language**:
- Device, Interface, Port, Link
- Registration, Discovery, Identification
- Online, Offline, Unknown (status)
- SNMP, SSH (protocols)

**Responsibilities**:
- Device registration (manual and automated)
- Network discovery and device identification
- Device status tracking
- Interface and port management
- Device credential storage

**Team**: Core platform team
**Database**: `artifex_device_management`
**Events Published**:
- `DeviceRegisteredEvent`
- `DeviceDiscoveredEvent`
- `DeviceStatusChangedEvent`
- `DeviceDeletedEvent`

---

### 2. Topology Management (Supporting Domain)
**Purpose**: Builds and maintains network topology maps

**Ubiquitous Language**:
- Link, Connection, Neighbor
- Topology, Network Map
- LLDP, CDP (discovery protocols)
- Uplink, Downlink

**Responsibilities**:
- Discover device interconnections
- Build topology graphs
- Detect topology changes
- Visualize network structure

**Team**: Topology team
**Database**: `artifex_topology_management`
**Events Published**:
- `LinkDiscoveredEvent`
- `TopologyChangedEvent`
- `NeighborDetectedEvent`

---

### 3. Overlay Network Management (Core Domain)
**Purpose**: Manages overlay networks (VPNs, tunnels, virtual networks)

**Ubiquitous Language**:
- Overlay, Tunnel, VPN
- VXLAN, GRE, IPsec
- Encapsulation, Underlay
- Virtual Network, Segment

**Responsibilities**:
- Create and manage overlay networks
- Configure tunnels and VPNs
- Manage virtual network segments
- Monitor overlay health

**Team**: SDN team
**Database**: `artifex_overlay_network`
**Events Published**:
- `OverlayCreatedEvent`
- `TunnelEstablishedEvent`
- `TunnelDownEvent`

---

### 4. Monitoring (Supporting Domain)
**Purpose**: Collects metrics, monitors health, and generates alerts

**Ubiquitous Language**:
- Metric, Measurement, Sample
- Alert, Threshold, Trigger
- Health Check, Availability
- Performance, Utilization

**Responsibilities**:
- Collect device metrics (CPU, memory, bandwidth)
- Monitor service health
- Generate alerts on threshold violations
- Store time-series data

**Team**: Observability team
**Database**: `artifex_monitoring`
**Events Published**:
- `MetricCollectedEvent`
- `AlertTriggeredEvent`
- `HealthCheckFailedEvent`

---

### 5. Configuration Management (Supporting Domain)
**Purpose**: Manages device configurations and compliance

**Ubiquitous Language**:
- Configuration, Config Template
- Compliance, Policy
- Baseline, Drift
- Rollback, Versioning

**Responsibilities**:
- Store device configurations
- Apply configuration templates
- Detect configuration drift
- Version control for configs
- Compliance checking

**Team**: Configuration team
**Database**: `artifex_configuration_management`
**Events Published**:
- `ConfigurationChangedEvent`
- `ComplianceViolationEvent`
- `ConfigurationAppliedEvent`

---

### 6. Identity & Access Management (Generic Subdomain)
**Purpose**: Manages users, authentication, and authorization

**Ubiquitous Language**:
- User, Role, Permission
- Authentication, Authorization
- API Key, Token
- Multi-Factor Authentication (MFA)

**Responsibilities**:
- User authentication
- Role-based access control (RBAC)
- API key management
- Audit logging for access

**Team**: Security team
**Database**: `artifex_identity`
**Events Published**:
- `UserAuthenticatedEvent`
- `AccessDeniedEvent`
- `RoleAssignedEvent`

---

### 7. Event Store (Supporting Domain)
**Purpose**: Central event log for all domain events

**Ubiquitous Language**:
- Event, Event Stream
- Aggregate ID, Version
- Snapshot, Replay

**Responsibilities**:
- Persist all domain events
- Enable event sourcing
- Support event replay
- Provide audit trail

**Team**: Platform team
**Database**: `artifex_event_store`

---

### 8. Node Management (Supporting Domain)
**Purpose**: Manages distributed node agents for remote sites

**Ubiquitous Language**:
- Node, Agent, Site
- Registration, Heartbeat
- Local Discovery, Remote Sync

**Responsibilities**:
- Register and track node agents
- Monitor agent health
- Coordinate remote discovery
- Sync local findings to central

**Team**: Platform team
**Database**: `artifex_node_management`
**Events Published**:
- `NodeRegisteredEvent`
- `NodeHeartbeatEvent`
- `NodeOfflineEvent`

---

## Context Relationships (Context Map)

### Device Management ↔ Topology Management
**Pattern**: **Customer/Supplier** (Device Management is Supplier)

```
┌─────────────────────┐
│ Device Management   │
│   (Supplier)        │
└──────────┬──────────┘
           │ Published Language:
           │ - DeviceRegisteredEvent
           │ - DeviceStatusChangedEvent
           │
           ▼
┌─────────────────────┐
│ Topology Management │
│   (Customer)        │
└─────────────────────┘
```

**Integration**:
- Topology consumes device events to update topology
- Topology calls Device Management API for device details
- Shared understanding of Device entity via events

**Published Language**: Domain events (DeviceRegisteredEvent, etc.)

---

### Device Management ↔ Monitoring
**Pattern**: **Customer/Supplier** (Device Management is Supplier)

```
┌─────────────────────┐
│ Device Management   │
│   (Supplier)        │
└──────────┬──────────┘
           │ Published Language:
           │ - DeviceRegisteredEvent
           │ - DeviceStatusChangedEvent
           │
           ▼
┌─────────────────────┐
│    Monitoring       │
│   (Customer)        │
└─────────────────────┘
```

**Integration**:
- Monitoring subscribes to device events to start/stop monitoring
- Monitoring queries Device Management for device credentials
- Monitoring publishes metrics and alerts

**Published Language**: Domain events

---

### Device Management ↔ Configuration Management
**Pattern**: **Partnership**

```
┌─────────────────────┐          ┌─────────────────────┐
│ Device Management   │◄────────►│ Configuration Mgmt  │
│                     │  Events  │                     │
└─────────────────────┘          └─────────────────────┘
```

**Integration**:
- Device Management notifies Configuration Mgmt of new devices
- Configuration Mgmt applies configs and notifies Device Management
- Both need to stay synchronized on device state

**Communication**: Bi-directional events + API calls

---

### Device Management ↔ Overlay Network Management
**Pattern**: **Shared Kernel**

```
┌─────────────────────────────────────────┐
│          Shared Kernel                  │
│  - Common device model                  │
│  - Shared value objects (IpAddress)     │
│  - Shared enums (DeviceType)            │
└─────────────┬───────────────┬───────────┘
              │               │
    ┌─────────▼──────┐  ┌────▼──────────┐
    │ Device Mgmt    │  │ Overlay Mgmt  │
    └────────────────┘  └───────────────┘
```

**Integration**:
- Both share common device understanding
- Shared libraries for device-related value objects
- Close collaboration required

**Shared Code**: `artifex.shared.domain`

---

### Device Management ↔ Identity & Access Management
**Pattern**: **Conformist** (Device Management conforms to Identity)

```
┌─────────────────────┐
│  Identity & Access  │
│   (Upstream)        │
└──────────┬──────────┘
           │ Authentication API
           │ (Device Mgmt must conform)
           │
           ▼
┌─────────────────────┐
│ Device Management   │
│   (Downstream)      │
└─────────────────────┘
```

**Integration**:
- Device Management uses Identity for authentication/authorization
- Device Management has no influence on Identity's model
- One-way dependency

---

### Device Management ↔ Node Management
**Pattern**: **Customer/Supplier** (Node Management is Supplier)

```
┌─────────────────────┐
│  Node Management    │
│   (Supplier)        │
└──────────┬──────────┘
           │ Node discovery API
           │
           ▼
┌─────────────────────┐
│ Device Management   │
│   (Customer)        │
└─────────────────────┘
```

**Integration**:
- Node agents report discovered devices to Device Management
- Device Management queries Node Management for agent status
- Nodes act as distributed discovery clients

---

### All Contexts ↔ Event Store
**Pattern**: **Conformist** (All contexts conform to Event Store)

```
    ┌────────────────────────┐
    │     Event Store        │
    │    (Central Hub)       │
    └───┬──────┬──────┬──────┘
        │      │      │
┌───────▼──┐ ┌▼──────▼──┐ ┌──▼────────┐
│ Device   │ │Topology  │ │Monitoring │
│   Mgmt   │ │  Mgmt    │ │           │
└──────────┘ └──────────┘ └───────────┘
```

**Integration**:
- All contexts publish events to Event Store
- Event Store provides single source of truth
- All contexts conform to event schema

---

### External System: SNMP Libraries
**Pattern**: **Anti-Corruption Layer** (ACL)

```
External World          ACL Boundary           Domain
┌──────────────┐       ┌─────────────┐       ┌──────────────┐
│ SharpSnmpLib │──────►│ SnmpAdapter │──────►│Device Mgmt   │
│ (3rd party)  │       │   (ACL)     │       │  Domain      │
└──────────────┘       └─────────────┘       └──────────────┘
```

**Integration**:
- SnmpAdapter translates SNMP library types to domain types
- Domain never depends on external SNMP library directly
- ACL protects domain from external changes

**Location**: `artifex.device-management.infrastructure/communication/acl`

---

## Context Map Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Artifex Context Map                           │
└─────────────────────────────────────────────────────────────────┘

                    ┌──────────────────┐
                    │  Identity & IAM  │
                    │  (Generic)       │
                    └────────┬─────────┘
                             │ Conformist
                             │
                   ┌─────────▼─────────┐
                   │                   │
         ┌─────────┤ Device Management ├─────────┐
         │         │   (Core Domain)   │         │
         │         └─────────┬─────────┘         │
         │                   │                   │
         │                   │                   │
   Customer/           Shared Kernel        Customer/
   Supplier                 │               Supplier
         │                  │                    │
         │         ┌────────▼────────┐           │
         │         │ Overlay Network │           │
         │         │  (Core Domain)  │           │
         │         └─────────────────┘           │
         │                                       │
    ┌────▼──────────┐                  ┌────────▼─────────┐
    │  Topology     │                  │   Monitoring     │
    │  Management   │                  │   (Supporting)   │
    │  (Supporting) │                  └──────────────────┘
    └───────────────┘                           │
                                                │
                                      ┌─────────▼──────────┐
                                      │  Configuration     │
                                      │   Management       │
                                      │  (Supporting)      │
                                      └────────────────────┘

         ┌────────────────────────────────────┐
         │        Event Store                 │
         │      (Central Event Log)           │
         │   All contexts publish events      │
         └────────────────────────────────────┘

         ┌────────────────────────────────────┐
         │     Node Management                │
         │   (Distributed Agents)             │
         │   Feeds Device Management          │
         └────────────────────────────────────┘

External:
         ┌────────────────┐   ACL   ┌────────────────┐
         │  SNMP Libraries├────────►│  SnmpAdapter   │
         └────────────────┘         └────────────────┘
```

---

## Integration Patterns Summary

| Pattern | Description | Example in Artifex |
|---------|-------------|--------------------|
| **Shared Kernel** | Shared code/models between contexts | Device Mgmt ↔ Overlay Network |
| **Customer/Supplier** | Upstream serves downstream | Device Mgmt → Topology |
| **Conformist** | Downstream conforms to upstream | All → Identity & IAM |
| **Partnership** | Mutual dependency, close collaboration | Device Mgmt ↔ Configuration |
| **Anti-Corruption Layer** | Isolate domain from external systems | SNMP Libraries → Domain |
| **Published Language** | Well-defined integration contract | Domain Events |

---

## Communication Protocols

### Synchronous (Request/Response)
- **HTTP/REST**: Direct service-to-service calls (via Consul discovery)
- **Use When**: Immediate response needed, query operations

### Asynchronous (Event-Driven)
- **MassTransit + RabbitMQ**: Domain event publishing
- **Use When**: Event notification, eventual consistency acceptable

---

## Guidelines for Adding New Contexts

When adding a new bounded context:

1. **Define boundaries**: What is inside/outside this context?
2. **Identify ubiquitous language**: What terms are specific to this context?
3. **Choose integration pattern**: How does it relate to existing contexts?
4. **Define events**: What domain events does it publish/consume?
5. **Document in this map**: Update this document and diagrams
6. **Create ACL if needed**: Protect domain from external dependencies

---

## References

- **Book**: "Domain-Driven Design" by Eric Evans
- **Book**: "Implementing Domain-Driven Design" by Vaughn Vernon
- **Pattern**: Context Mapping Patterns - https://www.domainlanguage.com/ddd/patterns/

---

**Next Review Date**: 2026-03-01
