using Xunit;

namespace Maestro.Shared.Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Result_Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Result_Failure_ShouldCreateFailureResult()
    {
        // Act
        var result = Result.Failure("Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Contains("Error message", result.Errors);
    }

    [Fact]
    public void Result_Failure_WithMultipleErrors_ShouldCreateFailureResultWithAllErrors()
    {
        // Act
        var result = Result.Failure(new[] { "Error 1", "Error 2" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ResultT_Success_ShouldCreateSuccessResultWithValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ResultT_Failure_ShouldCreateFailureResultWithDefaultValue()
    {
        // Act
        var result = Result<int>.Failure("Error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(default(int), result.Value);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ResultT_ImplicitConversion_FromResult_ShouldConvertCorrectly()
    {
        // Arrange
        var baseResult = Result.Failure("Error");

        // Act
        Result<int> result = baseResult;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(result.Errors, baseResult.Errors);
    }
}
