using Xunit;

namespace Artifex.Shared.Domain.Tests;

public class EntityTests
{
    private class TestEntity : Entity<int>
    {
        public TestEntity(int id) : base(id) { }
    }

    [Fact]
    public void Entity_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void Entity_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
    }

    [Fact]
    public void Entity_GetHashCode_ShouldBeBasedOnId()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act & Assert
        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void Entity_Transient_ShouldNotBeEqualToAnotherTransient()
    {
        // Arrange
        var entity1 = new TestEntity(0);
        var entity2 = new TestEntity(0);

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
    }
}
