using Maestro.Shared.Presentation.Responses;
using Xunit;

namespace Maestro.Shared.Presentation.Tests;

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Act
        var response = ApiResponse<string>.SuccessResponse("Test data", "Operation successful");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Test data", response.Data);
        Assert.Equal("Operation successful", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void ApiResponse_FailureResponse_ShouldCreateFailedResponse()
    {
        // Act
        var response = ApiResponse<string>.FailureResponse("Error occurred", "Operation failed");

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Operation failed", response.Message);
        Assert.Single(response.Errors);
        Assert.Contains("Error occurred", response.Errors);
    }

    [Fact]
    public void ApiResponse_FailureResponseWithMultipleErrors_ShouldContainAllErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var response = ApiResponse<string>.FailureResponse(errors);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(3, response.Errors.Count);
        Assert.Contains("Error 1", response.Errors);
        Assert.Contains("Error 2", response.Errors);
        Assert.Contains("Error 3", response.Errors);
    }

    [Fact]
    public void ApiResponseWithoutData_SuccessResponse_ShouldCreateSuccessfulResponse()
    {
        // Act
        var response = ApiResponse.SuccessResponse("Success");

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Success", response.Message);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void ApiResponseWithoutData_FailureResponse_ShouldCreateFailedResponse()
    {
        // Act
        var response = ApiResponse.FailureResponse("Error");

        // Assert
        Assert.False(response.Success);
        Assert.Single(response.Errors);
        Assert.Contains("Error", response.Errors);
    }
}
