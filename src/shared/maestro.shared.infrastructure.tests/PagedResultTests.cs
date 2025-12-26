using Maestro.Shared.Api.DTOs;
using Xunit;

namespace Maestro.Shared.Application.Tests;

public class PagedResultTests
{
    [Fact]
    public void PagedResult_WithItems_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };

        // Act
        var result = new PagedResult<int>(items, totalCount: 25, pageNumber: 1, pageSize: 5);

        // Assert
        Assert.Equal(5, result.TotalPages);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public void PagedResult_FirstPage_ShouldNotHavePreviousPage()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>(items, totalCount: 10, pageNumber: 1, pageSize: 3);

        // Assert
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_LastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>(items, totalCount: 10, pageNumber: 4, pageSize: 3);

        // Assert
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_MiddlePage_ShouldHaveBothPages()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var result = new PagedResult<int>(items, totalCount: 10, pageNumber: 2, pageSize: 3);

        // Assert
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_Empty_ShouldReturnEmptyResult()
    {
        // Act
        var result = PagedResult<int>.Empty();

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.PageNumber);
        Assert.Equal(0, result.PageSize);
    }
}
