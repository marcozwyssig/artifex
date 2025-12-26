# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Maestro.sln", "./"]
COPY ["src/shared/maestro.shared.domain/maestro.shared.domain.csproj", "src/shared/maestro.shared.domain/"]
COPY ["src/shared/maestro.shared.application/maestro.shared.application.csproj", "src/shared/maestro.shared.application/"]
COPY ["src/shared/maestro.shared.infrastructure/maestro.shared.infrastructure.csproj", "src/shared/maestro.shared.infrastructure/"]
COPY ["src/shared/maestro.shared.presentation/maestro.shared.presentation.csproj", "src/shared/maestro.shared.presentation/"]
COPY ["src/services/device-management/maestro.device-management.domain/maestro.device-management.domain.csproj", "src/services/device-management/maestro.device-management.domain/"]
COPY ["src/services/device-management/maestro.device-management.application/maestro.device-management.application.csproj", "src/services/device-management/maestro.device-management.application/"]
COPY ["src/services/device-management/maestro.device-management.infrastructure/maestro.device-management.infrastructure.csproj", "src/services/device-management/maestro.device-management.infrastructure/"]
COPY ["src/services/device-management/maestro.device-management.presentation/api/maestro.device-management.presentation.csproj", "src/services/device-management/maestro.device-management.presentation/api/"]

# Restore dependencies
RUN dotnet restore "src/services/device-management/maestro.device-management.presentation/api/maestro.device-management.presentation.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/services/device-management/maestro.device-management.presentation/api"
RUN dotnet build "maestro.device-management.presentation.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "maestro.device-management.presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install network utilities for SNMP and ping
RUN apt-get update && apt-get install -y \
    iputils-ping \
    snmp \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 5001
EXPOSE 5002

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "maestro.device-management.presentation.dll"]
