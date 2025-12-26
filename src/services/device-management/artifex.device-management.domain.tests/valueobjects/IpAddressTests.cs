using Artifex.DeviceManagement.Domain.ValueObjects;
using Xunit;

namespace Artifex.DeviceManagement.Domain.Tests.ValueObjects;

public class IpAddressTests
{
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void Create_WithValidIpAddress_ShouldSucceed(string ipAddress)
    {
        // Act
        var result = IpAddress.Create(ipAddress);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ipAddress, result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyIpAddress_ShouldFail(string ipAddress)
    {
        // Act
        var result = IpAddress.Create(ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("cannot be empty", result.Errors.First());
    }

    [Theory]
    [InlineData("256.1.1.1")]
    [InlineData("192.168.1")]
    [InlineData("invalid")]
    public void Create_WithInvalidIpAddress_ShouldFail(string ipAddress)
    {
        // Act
        var result = IpAddress.Create(ipAddress);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid IP address format", result.Errors.First());
    }

    [Fact]
    public void IpAddress_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var ip1 = IpAddress.Create("192.168.1.1").Value!;
        var ip2 = IpAddress.Create("192.168.1.1").Value!;

        // Assert
        Assert.Equal(ip1, ip2);
    }
}
