using Maestro.DeviceManagement.Domain.ValueObjects;
using Xunit;

namespace Maestro.DeviceManagement.Domain.Tests.ValueObjects;

public class MacAddressTests
{
    [Theory]
    [InlineData("00:11:22:33:44:55")]
    [InlineData("AA:BB:CC:DD:EE:FF")]
    [InlineData("00-11-22-33-44-55")]
    public void Create_WithValidMacAddress_ShouldSucceed(string macAddress)
    {
        // Act
        var result = MacAddress.Create(macAddress);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyMacAddress_ShouldFail(string macAddress)
    {
        // Act
        var result = MacAddress.Create(macAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("cannot be empty", result.Errors.First());
    }

    [Theory]
    [InlineData("00:11:22:33:44")]
    [InlineData("00:11:22:33:44:55:66")]
    [InlineData("invalid")]
    public void Create_WithInvalidMacAddress_ShouldFail(string macAddress)
    {
        // Act
        var result = MacAddress.Create(macAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid MAC address format", result.Errors.First());
    }

    [Fact]
    public void Create_ShouldNormalizeToUpperCase()
    {
        // Act
        var result = MacAddress.Create("aa:bb:cc:dd:ee:ff");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AA:BB:CC:DD:EE:FF", result.Value!.Value);
    }
}
