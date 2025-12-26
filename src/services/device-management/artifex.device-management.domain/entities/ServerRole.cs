using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Entities;

/// <summary>
/// Server role entity - represents server functionality
/// </summary>
public class ServerRole : DeviceRole
{
    public ServerType Type { get; private set; }
    public bool IsVirtualized { get; private set; }
    public bool IsHighAvailability { get; private set; }
    public int NumberOfCpuCores { get; private set; }
    public long MemoryInGB { get; private set; }
    public string ServicesJson { get; private set; } // Store as JSON

    // EF Core requires a parameterless constructor
    private ServerRole() : base()
    {
        ServicesJson = "[]";
    }

    private ServerRole(
        Guid id,
        Guid deviceId,
        ServerType type,
        bool isVirtualized,
        bool isHighAvailability,
        int numberOfCpuCores,
        long memoryInGB,
        string servicesJson,
        string? description = null)
        : base(id, deviceId, "Server", description)
    {
        Type = type;
        IsVirtualized = isVirtualized;
        IsHighAvailability = isHighAvailability;
        NumberOfCpuCores = numberOfCpuCores;
        MemoryInGB = memoryInGB;
        ServicesJson = servicesJson;
    }

    public static Result<ServerRole> Create(
        Guid deviceId,
        ServerType type,
        bool isVirtualized = false,
        bool isHighAvailability = false,
        int numberOfCpuCores = 1,
        long memoryInGB = 1,
        IReadOnlyCollection<string>? services = null,
        string? description = null)
    {
        if (numberOfCpuCores < 1)
        {
            return Result.Failure<ServerRole>("Server must have at least one CPU core");
        }

        if (memoryInGB < 1)
        {
            return Result.Failure<ServerRole>("Server must have at least 1GB of memory");
        }

        var servicesJson = System.Text.Json.JsonSerializer.Serialize(services ?? Array.Empty<string>());

        var role = new ServerRole(
            Guid.NewGuid(),
            deviceId,
            type,
            isVirtualized,
            isHighAvailability,
            numberOfCpuCores,
            memoryInGB,
            servicesJson,
            description);

        var validationResult = role.Validate();
        if (validationResult.IsFailure)
        {
            return Result.Failure<ServerRole>(validationResult.Error);
        }

        return Result.Success(role);
    }

    public override Result Validate()
    {
        return Result.Success();
    }

    public override IReadOnlyCollection<int> GetRequiredPorts()
    {
        var ports = new List<int> { 22 };

        switch (Type)
        {
            case ServerType.WebServer:
                ports.AddRange(new[] { 80, 443 });
                break;
            case ServerType.DatabaseServer:
                ports.AddRange(new[] { 3306, 5432 });
                break;
            case ServerType.DnsServer:
                ports.Add(53);
                break;
            case ServerType.DhcpServer:
                ports.AddRange(new[] { 67, 68 });
                break;
            case ServerType.FileServer:
                ports.AddRange(new[] { 445, 2049 });
                break;
            case ServerType.MailServer:
                ports.AddRange(new[] { 25, 110, 143, 465, 587, 993, 995 });
                break;
        }

        return ports.Distinct().ToList().AsReadOnly();
    }

    public IReadOnlyCollection<string> GetServices()
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(ServicesJson) ?? new List<string>();
    }
}

/// <summary>
/// Type of server
/// </summary>
public enum ServerType
{
    Unknown = 0,
    WebServer = 1,
    ApplicationServer = 2,
    DatabaseServer = 3,
    FileServer = 4,
    MailServer = 5,
    DnsServer = 6,
    DhcpServer = 7,
    ProxyServer = 8,
    MonitoringServer = 9,
    BackupServer = 10
}
