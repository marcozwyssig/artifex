using Artifex.Shared.Ui.Responses;
using Xunit;

namespace Artifex.Shared.Ui.Tests;

public class CqrsResponseTests
{
    [Fact]
    public void CqrsResponse_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Act
        var response = CqrsResponse<string>.SuccessResponse("Test data", "Operation successful");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Test data", response.Data);
        Assert.Equal("Operation successful", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void CqrsResponse_FailureResponse_ShouldCreateFailedResponse()
    {
        // Act
        var response = CqrsResponse<string>.FailureResponse("Error occurred", "Operation failed");

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Operation failed", response.Message);
        Assert.Single(response.Errors);
        Assert.Contains("Error occurred", response.Errors);
    }

    [Fact]
    public void CqrsResponse_FailureResponseWithMultipleErrors_ShouldContainAllErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var response = CqrsResponse<string>.FailureResponse(errors);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(3, response.Errors.Count);
        Assert.Contains("Error 1", response.Errors);
        Assert.Contains("Error 2", response.Errors);
        Assert.Contains("Error 3", response.Errors);
    }

    [Fact]
    public void CqrsResponseWithoutData_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Act
        var response = CqrsResponse.SuccessResponse("Success");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Success", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void CqrsResponseWithoutData_FailureResponse_ShouldCreateFailedResponse()
    {
        // Act
        var response = CqrsResponse.FailureResponse("Error");

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Contains("Error", response.Errors);
    }
}
