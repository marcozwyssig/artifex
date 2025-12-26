using Artifex.DeviceManagement.Domain.Aggregates;
using Artifex.DeviceManagement.Domain.Entities;
using Artifex.DeviceManagement.Domain.Enums;
using Artifex.DeviceManagement.Domain.ValueObjects;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.Factories;

/// <summary>
/// Factory for creating Device aggregates with complex initialization
/// </summary>
public class DeviceFactory
{
    /// <summary>
    /// Creates a device from manual registration
    /// </summary>
    public Result<Device> CreateFromManualRegistration(
        string hostname,
        string ipAddress,
        DeviceType type,
        Vendor vendor,
        string username,
        string password,
        NetworkSegment networkSegment = NetworkSegment.Unknown,
        string? location = null,
        string? description = null)
    {
        var ipResult = IpAddress.Create(ipAddress);
        if (ipResult.IsFailure)
        {
            return Result.Failure<Device>(ipResult.Error);
        }

        var credentialsResult = Credentials.Create(username, password);
        if (credentialsResult.IsFailure)
        {
            return Result.Failure<Device>(credentialsResult.Error);
        }

        var deviceResult = Device.Create(
            hostname,
            ipResult.Value,
            type,
            vendor,
            credentialsResult.Value,
            networkSegment);

        if (deviceResult.IsFailure)
        {
            return deviceResult;
        }

        var device = deviceResult.Value;

        // Set optional properties
        if (!string.IsNullOrWhiteSpace(location) || !string.IsNullOrWhiteSpace(description))
        {
            device.UpdateInformation(description: description, location: location);
        }

        return Result<Device>.Success(device);
    }

    /// <summary>
    /// Creates a device from network discovery
    /// </summary>
    public Result<Device> CreateFromDiscovery(
        string hostname,
        string ipAddress,
        DeviceType type,
        Vendor vendor,
        NetworkSegment networkSegment,
        string? macAddress = null,
        string? model = null,
        string? serialNumber = null,
        string? softwareVersion = null,
        IEnumerable<Interface>? interfaces = null)
    {
        var ipResult = IpAddress.Create(ipAddress);
        if (ipResult.IsFailure)
        {
            return Result.Failure<Device>(ipResult.Error);
        }

        // For discovered devices, use default credentials
        var credentialsResult = Credentials.Create("admin", "admin");
        if (credentialsResult.IsFailure)
        {
            return Result.Failure<Device>(credentialsResult.Error);
        }

        var deviceResult = Device.Create(
            hostname,
            ipResult.Value,
            type,
            vendor,
            credentialsResult.Value,
            networkSegment);

        if (deviceResult.IsFailure)
        {
            return deviceResult;
        }

        var device = deviceResult.Value;

        // Set MAC address if provided
        if (!string.IsNullOrWhiteSpace(macAddress))
        {
            var macResult = MacAddress.Create(macAddress);
            if (macResult.IsSuccess)
            {
                device.UpdateMacAddress(macResult.Value);
            }
        }

        // Update information from discovery
        if (!string.IsNullOrWhiteSpace(model) ||
            !string.IsNullOrWhiteSpace(serialNumber) ||
            !string.IsNullOrWhiteSpace(softwareVersion))
        {
            device.UpdateInformation(
                model: model,
                serialNumber: serialNumber,
                softwareVersion: softwareVersion);
        }

        // Add interfaces if provided
        if (interfaces != null)
        {
            foreach (var iface in interfaces)
            {
                device.AddInterface(iface);
            }
        }

        return Result<Device>.Success(device);
    }

    /// <summary>
    /// Creates a device from SNMP identification
    /// </summary>
    public Result<Device> CreateFromSnmpIdentification(
        string hostname,
        string ipAddress,
        string systemDescription,
        string systemObjectId,
        NetworkSegment networkSegment,
        string? macAddress = null)
    {
        // Parse system description to determine vendor and type
        var (vendor, type) = ParseSystemDescription(systemDescription, systemObjectId);

        return CreateFromDiscovery(
            hostname,
            ipAddress,
            type,
            vendor,
            networkSegment,
            macAddress);
    }

    /// <summary>
    /// Parses SNMP system description to determine vendor and device type
    /// </summary>
    private (Vendor vendor, DeviceType type) ParseSystemDescription(
        string systemDescription,
        string systemObjectId)
    {
        // Default values
        var vendor = Vendor.Unknown;
        var type = DeviceType.Unknown;

        var descLower = systemDescription.ToLowerInvariant();
        var oidLower = systemObjectId.ToLowerInvariant();

        // Detect vendor
        if (descLower.Contains("cisco") || oidLower.Contains("1.3.6.1.4.1.9"))
        {
            vendor = Vendor.Cisco;
        }
        else if (descLower.Contains("juniper") || oidLower.Contains("1.3.6.1.4.1.2636"))
        {
            vendor = Vendor.Juniper;
        }
        else if (descLower.Contains("hp") || descLower.Contains("hewlett") ||
                 oidLower.Contains("1.3.6.1.4.1.11"))
        {
            vendor = Vendor.HPE;
        }
        else if (descLower.Contains("arista") || oidLower.Contains("1.3.6.1.4.1.30065"))
        {
            vendor = Vendor.Arista;
        }

        // Detect device type based on description
        if (descLower.Contains("switch") || descLower.Contains("catalyst"))
        {
            type = DeviceType.Switch;
        }
        else if (descLower.Contains("router") || descLower.Contains("asr") ||
                 descLower.Contains("isr"))
        {
            type = DeviceType.Router;
        }
        else if (descLower.Contains("firewall") || descLower.Contains("asa") ||
                 descLower.Contains("srx"))
        {
            type = DeviceType.Firewall;
        }
        else if (descLower.Contains("wireless") || descLower.Contains("wlc") ||
                 descLower.Contains("access point") || descLower.Contains("ap"))
        {
            type = DeviceType.AccessPoint;
        }
        else if (descLower.Contains("server") || descLower.Contains("linux") ||
                 descLower.Contains("windows"))
        {
            type = DeviceType.Server;
        }

        return (vendor, type);
    }
}
