# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Maestro.sln", "./"]
COPY ["src/shared/maestro.shared.domain/maestro.shared.domain.csproj", "src/shared/maestro.shared.domain/"]
COPY ["src/shared/maestro.shared.application/maestro.shared.application.csproj", "src/shared/maestro.shared.application/"]
COPY ["src/shared/maestro.shared.infrastructure/maestro.shared.infrastructure.csproj", "src/shared/maestro.shared.infrastructure/"]
COPY ["src/shared/maestro.shared.presentation/maestro.shared.presentation.csproj", "src/shared/maestro.shared.presentation/"]
COPY ["src/applications/node-agent/maestro.node-agent.domain/maestro.node-agent.domain.csproj", "src/applications/node-agent/maestro.node-agent.domain/"]
COPY ["src/applications/node-agent/maestro.node-agent.application/maestro.node-agent.application.csproj", "src/applications/node-agent/maestro.node-agent.application/"]
COPY ["src/applications/node-agent/maestro.node-agent.infrastructure/maestro.node-agent.infrastructure.csproj", "src/applications/node-agent/maestro.node-agent.infrastructure/"]
COPY ["src/applications/node-agent/maestro.node-agent.presentation/api/maestro.node-agent.presentation.csproj", "src/applications/node-agent/maestro.node-agent.presentation/api/"]

# Restore dependencies
RUN dotnet restore "src/applications/node-agent/maestro.node-agent.presentation/api/maestro.node-agent.presentation.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/applications/node-agent/maestro.node-agent.presentation/api"
RUN dotnet build "maestro.node-agent.presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "maestro.node-agent.presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install network utilities for SNMP and ping
RUN apt-get update && apt-get install -y \
    iputils-ping \
    snmp \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5003

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "maestro.node-agent.presentation.dll"]
