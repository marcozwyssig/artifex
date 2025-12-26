using System.Text.RegularExpressions;
using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.ValueObjects;

/// <summary>
/// MAC Address value object
/// </summary>
public class MacAddress : ValueObject
{
    private static readonly Regex MacAddressRegex = new Regex(
        @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        RegexOptions.Compiled);

    public string Value { get; private set; }

    private MacAddress(string value)
    {
        Value = value.ToUpperInvariant();
    }

    public static Result<MacAddress> Create(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
        {
            return Result.Failure<MacAddress>("MAC address cannot be empty");
        }

        if (!MacAddressRegex.IsMatch(macAddress))
        {
            return Result.Failure<MacAddress>($"Invalid MAC address format: {macAddress}");
        }

        return Result.Success(new MacAddress(macAddress));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(MacAddress macAddress) => macAddress.Value;
}
