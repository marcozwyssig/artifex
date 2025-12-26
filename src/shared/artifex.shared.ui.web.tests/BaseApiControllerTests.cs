using Artifex.Shared.Domain;
using Artifex.Shared.Ui.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Artifex.Shared.Ui.Tests;

public class BaseApiControllerTests
{
    private class TestController : BaseApiController
    {
        public ActionResult TestHandleResult(Result result) => HandleResult(result);
        public ActionResult<string> TestHandleResultWithValue(Result<string> result) => HandleResult(result);
    }

    [Fact]
    public void HandleResult_WithSuccessResult_ShouldReturnOk()
    {
        // Arrange
        var controller = new TestController();
        var result = Result.Success();

        // Act
        var actionResult = controller.TestHandleResult(result);

        // Assert
        Assert.IsType<OkResult>(actionResult);
    }

    [Fact]
    public void HandleResult_WithFailureResult_ShouldReturnBadRequest()
    {
        // Arrange
        var controller = new TestController();
        var result = Result.Failure("Validation error");

        // Act
        var actionResult = controller.TestHandleResult(result);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult);
    }

    [Fact]
    public void HandleResult_WithNotFoundError_ShouldReturnNotFound()
    {
        // Arrange
        var controller = new TestController();
        var result = Result.Failure("Resource not found");

        // Act
        var actionResult = controller.TestHandleResult(result);

        // Assert
        Assert.IsType<NotFoundObjectResult>(actionResult);
    }

    [Fact]
    public void HandleResult_WithUnauthorizedError_ShouldReturnUnauthorized()
    {
        // Arrange
        var controller = new TestController();
        var result = Result.Failure("Unauthorized access");

        // Act
        var actionResult = controller.TestHandleResult(result);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(actionResult);
    }

    [Fact]
    public void HandleResultWithValue_WithSuccessResult_ShouldReturnOkWithValue()
    {
        // Arrange
        var controller = new TestController();
        var result = Result<string>.Success("Test value");

        // Act
        var actionResult = controller.TestHandleResultWithValue(result);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal("Test value", okResult.Value);
    }

    [Fact]
    public void HandleResultWithValue_WithFailureResult_ShouldReturnBadRequest()
    {
        // Arrange
        var controller = new TestController();
        var result = Result<string>.Failure("Error occurred");

        // Act
        var actionResult = controller.TestHandleResultWithValue(result);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}
