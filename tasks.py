"""
Artifex Network Management System - Build Automation (UPDATED ARCHITECTURE)
===========================================================================

Proper Layered Architecture:
- Domain: Pure business logic
- Application: Use cases, jobs, API coordination
- Infrastructure: Database, Communication (SNMP/SSH), Automation (Ansible)
- Ui: API controllers, UI

Structure:
- src/shared/ (renamed from building-blocks)
- src/services/{service}/
  - artifex.{service}.domain/
  - artifex.{service}.application/
  - artifex.{service}.infrastructure/
    - persistence/
    - communication/  (SNMP, SSH, HTTP, Python adapters)
    - automation/     (Ansible)
  - artifex.{service}.web/
    - api/
  - artifex.{service}.database/
"""

from invoke import task, Collection
import os
import sys
import platform
from pathlib import Path

# ============================================================================
# Configuration
# ============================================================================

PROJECT_ROOT = Path(__file__).parent
SRC_DIR = PROJECT_ROOT / "src"
SERVICES_DIR = SRC_DIR / "services"
SHARED_DIR = SRC_DIR / "shared"
WEB_DIR = SRC_DIR / "web" / "artifex.web.web"
DOCKER_COMPOSE_FILE = PROJECT_ROOT / "docker-compose.yml"

# ============================================================================
# Helper Functions
# ============================================================================

def print_success(message):
    """Print success message"""
    print(f"✓ {message}")

def print_error(message):
    """Print error message"""
    print(f"✗ {message}")

def print_info(message):
    """Print info message"""
    print(f"→ {message}")

# ============================================================================
# Build Tasks
# ============================================================================

@task
def clean(c):
    """Clean build artifacts"""
    print_info("Cleaning build artifacts...")
    c.run("dotnet clean Artifex.sln", warn=True)
    c.run("find . -type d -name 'bin' -o -name 'obj' | xargs rm -rf", warn=True)
    print_success("Build artifacts cleaned")

@task
def restore(c):
    """Restore NuGet packages"""
    print_info("Restoring NuGet packages...")
    c.run("dotnet restore Artifex.sln")
    print_success("NuGet packages restored")

@task
def build(c):
    """Build the entire solution"""
    print_info("Building Artifex solution...")
    c.run("dotnet build Artifex.sln -c Release")
    print_success("Solution built successfully")

@task
def build_device_management(c):
    """Build Device Management service"""
    print_info("Building Device Management service...")
    c.run("dotnet build src/services/device-management/artifex.device-management.web/cqrs/artifex.device-management.web.csproj -c Release")
    print_success("Device Management service built")

@task
def build_node_agent(c):
    """Build Node Agent application"""
    print_info("Building Node Agent...")
    c.run("dotnet build src/applications/node-agent/artifex.node-agent.web/cqrs/artifex.node-agent.web.csproj -c Release")
    print_success("Node Agent built")

@task(pre=[restore])
def build_all(c):
    """Restore and build all projects"""
    build(c)

# ============================================================================
# Test Tasks
# ============================================================================

@task
def test(c):
    """Run all tests"""
    print_info("Running tests...")
    c.run("dotnet test Artifex.sln --no-build -c Release")
    print_success("Tests completed")

@task
def test_unit(c):
    """Run unit tests"""
    print_info("Running unit tests...")
    c.run("dotnet test --filter Category=Unit --no-build -c Release")
    print_success("Unit tests completed")

@task
def test_integration(c):
    """Run integration tests"""
    print_info("Running integration tests...")
    c.run("dotnet test --filter Category=Integration --no-build -c Release")
    print_success("Integration tests completed")

# ============================================================================
# Database Tasks
# ============================================================================

@task
def db_migration_create(c, name, service="device-management"):
    """Create a new database migration"""
    print_info(f"Creating migration '{name}' for {service}...")
    project_path = f"src/services/{service}/artifex.{service}.infrastructure/artifex.{service}.infrastructure.csproj"
    startup_project = f"src/services/{service}/artifex.{service}.web/cqrs/artifex.{service}.web.csproj"
    c.run(f"dotnet ef migrations add {name} --project {project_path} --startup-project {startup_project}")
    print_success(f"Migration '{name}' created")

@task
def db_migration_apply(c, service="device-management"):
    """Apply database migrations"""
    print_info(f"Applying migrations for {service}...")
    startup_project = f"src/services/{service}/artifex.{service}.web/cqrs/artifex.{service}.web.csproj"
    c.run(f"dotnet ef database update --project {startup_project}")
    print_success("Migrations applied")

@task
def db_migration_rollback(c, migration_name, service="device-management"):
    """Rollback to a specific migration"""
    print_info(f"Rolling back to migration '{migration_name}' for {service}...")
    startup_project = f"src/services/{service}/artifex.{service}.web/cqrs/artifex.{service}.web.csproj"
    c.run(f"dotnet ef database update {migration_name} --project {startup_project}")
    print_success(f"Rolled back to migration '{migration_name}'")

# ============================================================================
# Docker Tasks
# ============================================================================

@task
def docker_build(c):
    """Build Docker images"""
    print_info("Building Docker images...")
    c.run("docker-compose build")
    print_success("Docker images built")

@task
def docker_up(c):
    """Start all services with docker-compose"""
    print_info("Starting services...")
    c.run("docker-compose up -d")
    print_success("Services started")

@task
def docker_down(c):
    """Stop all services"""
    print_info("Stopping services...")
    c.run("docker-compose down")
    print_success("Services stopped")

@task
def docker_logs(c, service=""):
    """View logs for services"""
    if service:
        c.run(f"docker-compose logs -f {service}")
    else:
        c.run("docker-compose logs -f")

@task
def docker_restart(c, service=""):
    """Restart services"""
    if service:
        print_info(f"Restarting {service}...")
        c.run(f"docker-compose restart {service}")
        print_success(f"{service} restarted")
    else:
        print_info("Restarting all services...")
        c.run("docker-compose restart")
        print_success("All services restarted")

@task
def docker_clean(c):
    """Remove all containers, volumes, and images"""
    print_info("Cleaning Docker resources...")
    c.run("docker-compose down -v --rmi all")
    print_success("Docker resources cleaned")

# ============================================================================
# Run Tasks
# ============================================================================

@task
def run_device_management(c):
    """Run Device Management service locally"""
    print_info("Running Device Management service...")
    c.run("dotnet run --project src/services/device-management/artifex.device-management.web/cqrs/artifex.device-management.web.csproj")

@task
def run_node_agent(c):
    """Run Node Agent locally"""
    print_info("Running Node Agent...")
    c.run("dotnet run --project src/applications/node-agent/artifex.node-agent.web/artifex.node-agent.web.csproj")

# ============================================================================
# Development Tasks
# ============================================================================

@task
def dev_setup(c):
    """Setup development environment"""
    print_info("Setting up development environment...")
    restore(c)
    build(c)
    print_info("Creating development databases...")
    c.run("docker-compose up -d postgres rabbitmq")
    import time
    time.sleep(10)  # Wait for databases to be ready
    print_success("Development environment ready")

@task
def format(c):
    """Format code using dotnet format"""
    print_info("Formatting code...")
    c.run("dotnet format Artifex.sln")
    print_success("Code formatted")

@task
def lint(c):
    """Lint code (check formatting)"""
    print_info("Linting code...")
    c.run("dotnet format Artifex.sln --verify-no-changes")
    print_success("Code lint passed")

# ============================================================================
# Structure Task
# ============================================================================

@task
def create_structure(c):
    """Create complete directory structure with proper layered architecture"""

    directories = [
        # ============================================
        # SHARED (Cross-cutting concerns)
        # ============================================
        "src/shared/artifex.shared.domain",
        "src/shared/artifex.shared.application",
        "src/shared/artifex.shared.infrastructure/event-bus",
        "src/shared/artifex.shared.infrastructure/persistence",
        "src/shared/artifex.shared.infrastructure/communication",
        "src/shared/artifex.shared.web/cqrs",
        "src/shared/artifex.shared.web/web",

        # ============================================
        # DEVICE MANAGEMENT SERVICE
        # ============================================
        # Domain Layer
        "src/services/device-management/artifex.device-management.domain/aggregates",
        "src/services/device-management/artifex.device-management.domain/entities",
        "src/services/device-management/artifex.device-management.domain/value-objects",
        "src/services/device-management/artifex.device-management.domain/events",
        "src/services/device-management/artifex.device-management.domain/interfaces",
        "src/services/device-management/artifex.device-management.domain/services",

        # Application Layer
        "src/services/device-management/artifex.device-management.application/commands",
        "src/services/device-management/artifex.device-management.application/queries",
        "src/services/device-management/artifex.device-management.application/jobs",
        "src/services/device-management/artifex.device-management.application/dtos",
        "src/services/device-management/artifex.device-management.application/services",

        # Infrastructure Layer
        "src/services/device-management/artifex.device-management.infrastructure/persistence/repositories",
        "src/services/device-management/artifex.device-management.infrastructure/persistence/configurations",
        "src/services/device-management/artifex.device-management.infrastructure/communication/snmp",
        "src/services/device-management/artifex.device-management.infrastructure/communication/ssh",
        "src/services/device-management/artifex.device-management.infrastructure/communication/http",
        "src/services/device-management/artifex.device-management.infrastructure/communication/python-adapter/src",
        "src/services/device-management/artifex.device-management.infrastructure/communication/python-adapter/tests",
        "src/services/device-management/artifex.device-management.infrastructure/automation/playbooks",
        "src/services/device-management/artifex.device-management.infrastructure/automation/roles/cisco-ios-xe",
        "src/services/device-management/artifex.device-management.infrastructure/automation/roles/cisco-nxos",
        "src/services/device-management/artifex.device-management.infrastructure/automation/roles/cisco-ios-xr",
        "src/services/device-management/artifex.device-management.infrastructure/external-services",

        # Ui Layer
        "src/services/device-management/artifex.device-management.web/cqrs/controllers",
        "src/services/device-management/artifex.device-management.web/cqrs/middleware",

        # Database
        "src/services/device-management/artifex.device-management.database/migrations",

        # ============================================
        # TOPOLOGY MANAGEMENT SERVICE
        # ============================================
        "src/services/topology-management/artifex.topology-management.domain/aggregates",
        "src/services/topology-management/artifex.topology-management.domain/value-objects",
        "src/services/topology-management/artifex.topology-management.domain/events",

        "src/services/topology-management/artifex.topology-management.application/commands",
        "src/services/topology-management/artifex.topology-management.application/queries",

        "src/services/topology-management/artifex.topology-management.infrastructure/persistence",
        "src/services/topology-management/artifex.topology-management.infrastructure/communication/lldp",
        "src/services/topology-management/artifex.topology-management.infrastructure/communication/cdp",
        "src/services/topology-management/artifex.topology-management.infrastructure/communication/python-worker/src",

        "src/services/topology-management/artifex.topology-management.web/cqrs/controllers",

        "src/services/topology-management/artifex.topology-management.database/migrations",

        # ============================================
        # OVERLAY NETWORK SERVICE
        # ============================================
        "src/services/overlay-network/artifex.overlay-network.domain/aggregates",
        "src/services/overlay-network/artifex.overlay-network.domain/value-objects",
        "src/services/overlay-network/artifex.overlay-network.domain/events",

        "src/services/overlay-network/artifex.overlay-network.application/commands",
        "src/services/overlay-network/artifex.overlay-network.application/queries",
        "src/services/overlay-network/artifex.overlay-network.application/services",

        "src/services/overlay-network/artifex.overlay-network.infrastructure/persistence",
        "src/services/overlay-network/artifex.overlay-network.infrastructure/communication/tunnel-management",
        "src/services/overlay-network/artifex.overlay-network.infrastructure/automation/playbooks",
        "src/services/overlay-network/artifex.overlay-network.infrastructure/automation/roles/vxlan",
        "src/services/overlay-network/artifex.overlay-network.infrastructure/automation/roles/vrf",

        "src/services/overlay-network/artifex.overlay-network.web/cqrs/controllers",

        "src/services/overlay-network/artifex.overlay-network.database/migrations",

        # ============================================
        # MONITORING SERVICE
        # ============================================
        "src/services/monitoring/artifex.monitoring.domain/aggregates",
        "src/services/monitoring/artifex.monitoring.domain/value-objects",

        "src/services/monitoring/artifex.monitoring.application/commands",
        "src/services/monitoring/artifex.monitoring.application/queries",

        "src/services/monitoring/artifex.monitoring.infrastructure/persistence/timescale",
        "src/services/monitoring/artifex.monitoring.infrastructure/communication/snmp-collector",
        "src/services/monitoring/artifex.monitoring.infrastructure/communication/system-metrics",
        "src/services/monitoring/artifex.monitoring.infrastructure/communication/python-collector/src",

        "src/services/monitoring/artifex.monitoring.web/cqrs/controllers",

        "src/services/monitoring/artifex.monitoring.database/migrations",

        # Add other services similarly...
        # Identity, NodeManagement, ConfigurationManagement, EventStore

        # ============================================
        # NODE AGENT
        # ============================================
        "src/node-agent/artifex.node-agent.domain",
        "src/node-agent/artifex.node-agent.application",
        "src/node-agent/artifex.node-agent.infrastructure/communication",
        "src/node-agent/artifex.node-agent.infrastructure/sync",
        "src/node-agent/artifex.node-agent.web/cqrs",

        # ============================================
        # WEB UI
        # ============================================
        "src/web/artifex.web.web/src/features/devices",
        "src/web/artifex.web.web/src/features/topology",
        "src/web/artifex.web.web/src/features/overlays",
        "src/web/artifex.web.web/src/features/monitoring",
        "src/web/artifex.web.web/src/shared/components",
        "src/web/artifex.web.web/src/shared/services",
        "src/web/artifex.web.web/public",

        # Tests
        "tests/unit",
        "tests/integration",
        "tests/e2e",

        # Scripts
        "scripts",
    ]

    for directory in directories:
        path = PROJECT_ROOT / directory
        path.mkdir(parents=True, exist_ok=True)
        print(f"✓ Created {directory}")

# ... rest of tasks.py

# Export namespace
namespace = Collection()

# Structure
namespace.add_task(create_structure)

# Build tasks
namespace.add_task(clean)
namespace.add_task(restore)
namespace.add_task(build)
namespace.add_task(build_device_management, name='build-device-management')
namespace.add_task(build_node_agent, name='build-node-agent')
namespace.add_task(build_all, name='build-all')

# Test tasks
namespace.add_task(test)
namespace.add_task(test_unit, name='test-unit')
namespace.add_task(test_integration, name='test-integration')

# Database tasks
namespace.add_task(db_migration_create, name='db-migration-create')
namespace.add_task(db_migration_apply, name='db-migration-apply')
namespace.add_task(db_migration_rollback, name='db-migration-rollback')

# Docker tasks
namespace.add_task(docker_build, name='docker-build')
namespace.add_task(docker_up, name='docker-up')
namespace.add_task(docker_down, name='docker-down')
namespace.add_task(docker_logs, name='docker-logs')
namespace.add_task(docker_restart, name='docker-restart')
namespace.add_task(docker_clean, name='docker-clean')

# Run tasks
namespace.add_task(run_device_management, name='run-device-management')
namespace.add_task(run_node_agent, name='run-node-agent')

# Development tasks
namespace.add_task(dev_setup, name='dev-setup')
namespace.add_task(format)
namespace.add_task(lint)
