using Artifex.Shared.Cqrs.DTOs;
using Xunit;

namespace Artifex.Shared.Application.Tests;

public class PagedQueryTests
{
    private class TestPagedQuery : PagedQuery { }

    [Fact]
    public void PagedQuery_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var query = new TestPagedQuery();

        // Assert
        Assert.Equal(1, query.PageNumber);
        Assert.Equal(10, query.PageSize);
        Assert.Equal(0, query.Skip);
    }

    [Fact]
    public void PagedQuery_PageSize_ShouldNotExceedMaximum()
    {
        // Act
        var query = new TestPagedQuery { PageSize = 200 };

        // Assert
        Assert.Equal(100, query.PageSize);
    }

    [Fact]
    public void PagedQuery_Skip_ShouldCalculateCorrectly()
    {
        // Act
        var query = new TestPagedQuery { PageNumber = 3, PageSize = 10 };

        // Assert
        Assert.Equal(20, query.Skip);
    }

    [Fact]
    public void PagedQuery_CustomPageSize_ShouldBeRespected()
    {
        // Act
        var query = new TestPagedQuery { PageSize = 25 };

        // Assert
        Assert.Equal(25, query.PageSize);
    }

    [Fact]
    public void PagedQuery_CalculateSkip_ForDifferentPages()
    {
        // Arrange & Act
        var query1 = new TestPagedQuery { PageNumber = 1, PageSize = 20 };
        var query2 = new TestPagedQuery { PageNumber = 2, PageSize = 20 };
        var query3 = new TestPagedQuery { PageNumber = 5, PageSize = 20 };

        // Assert
        Assert.Equal(0, query1.Skip);
        Assert.Equal(20, query2.Skip);
        Assert.Equal(80, query3.Skip);
    }
}
