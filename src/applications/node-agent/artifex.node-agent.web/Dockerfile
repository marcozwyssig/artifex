# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Artifex.sln", "./"]
COPY ["src/shared/artifex.shared.domain/artifex.shared.domain.csproj", "src/shared/artifex.shared.domain/"]
COPY ["src/shared/artifex.shared.application/artifex.shared.application.csproj", "src/shared/artifex.shared.application/"]
COPY ["src/shared/artifex.shared.infrastructure/artifex.shared.infrastructure.csproj", "src/shared/artifex.shared.infrastructure/"]
COPY ["src/shared/artifex.shared.presentation/artifex.shared.presentation.csproj", "src/shared/artifex.shared.presentation/"]
COPY ["src/applications/node-agent/artifex.node-agent.domain/artifex.node-agent.domain.csproj", "src/applications/node-agent/artifex.node-agent.domain/"]
COPY ["src/applications/node-agent/artifex.node-agent.application/artifex.node-agent.application.csproj", "src/applications/node-agent/artifex.node-agent.application/"]
COPY ["src/applications/node-agent/artifex.node-agent.infrastructure/artifex.node-agent.infrastructure.csproj", "src/applications/node-agent/artifex.node-agent.infrastructure/"]
COPY ["src/applications/node-agent/artifex.node-agent.presentation/api/artifex.node-agent.presentation.csproj", "src/applications/node-agent/artifex.node-agent.presentation/api/"]

# Restore dependencies
RUN dotnet restore "src/applications/node-agent/artifex.node-agent.presentation/api/artifex.node-agent.presentation.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/applications/node-agent/artifex.node-agent.presentation/api"
RUN dotnet build "artifex.node-agent.presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "artifex.node-agent.presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install network utilities for SNMP and ping
RUN apt-get update && apt-get install -y \
    iputils-ping \
    snmp \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5003

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "artifex.node-agent.presentation.dll"]
