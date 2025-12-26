# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Artifex.sln", "./"]
COPY ["src/shared/artifex.shared.domain/artifex.shared.domain.csproj", "src/shared/artifex.shared.domain/"]
COPY ["src/shared/artifex.shared.infrastructure/artifex.shared.infrastructure.csproj", "src/shared/artifex.shared.infrastructure/"]
COPY ["src/shared/artifex.shared.presentation/artifex.shared.presentation.csproj", "src/shared/artifex.shared.presentation/"]
COPY ["src/services/device-management/artifex.device-management.domain/artifex.device-management.domain.csproj", "src/services/device-management/artifex.device-management.domain/"]
COPY ["src/services/device-management/artifex.device-management.infrastructure/artifex.device-management.infrastructure.csproj", "src/services/device-management/artifex.device-management.infrastructure/"]
COPY ["src/services/device-management/artifex.device-management.presentation/api/artifex.device-management.presentation.csproj", "src/services/device-management/artifex.device-management.presentation/api/"]

# Restore dependencies
RUN dotnet restore "src/services/device-management/artifex.device-management.presentation/api/artifex.device-management.presentation.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/services/device-management/artifex.device-management.presentation/api"
RUN dotnet build "artifex.device-management.presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "artifex.device-management.presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install network utilities for SNMP and ping
RUN apt-get update && apt-get install -y \
    iputils-ping \
    snmp \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5001
EXPOSE 5002

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "artifex.device-management.presentation.dll"]
