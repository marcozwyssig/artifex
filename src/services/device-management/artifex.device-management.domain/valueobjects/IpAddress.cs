using System.Net;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.ValueObjects;

/// <summary>
/// IP Address value object
/// </summary>
public class IpAddress : ValueObject
{
    public string Value { get; private set; }

    private IpAddress(string value)
    {
        Value = value;
    }

    public static Result<IpAddress> Create(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return Result.Failure<IpAddress>("IP address cannot be empty");
        }

        if (!IPAddress.TryParse(ipAddress, out _))
        {
            return Result.Failure<IpAddress>($"Invalid IP address format: {ipAddress}");
        }

        return Result.Success(new IpAddress(ipAddress));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(IpAddress ipAddress) => ipAddress.Value;
}
