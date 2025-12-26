using Xunit;

namespace Maestro.Shared.Domain.Tests;

public class AggregateRootTests
{
    private class TestEvent : DomainEvent
    {
        public string Message { get; }

        public TestEvent(string message)
        {
            Message = message;
        }
    }

    private class TestAggregate : AggregateRoot<int>
    {
        public TestAggregate(int id) : base(id) { }

        public void DoSomething(string message)
        {
            AddDomainEvent(new TestEvent(message));
        }
    }

    [Fact]
    public void AggregateRoot_AddDomainEvent_ShouldAddEventToList()
    {
        // Arrange
        var aggregate = new TestAggregate(1);

        // Act
        aggregate.DoSomething("Test message");

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<TestEvent>(aggregate.DomainEvents.First());
    }

    [Fact]
    public void AggregateRoot_ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        aggregate.DoSomething("Test 1");
        aggregate.DoSomething("Test 2");

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AggregateRoot_DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregate(1);
        aggregate.DoSomething("Test");

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<DomainEvent>>(aggregate.DomainEvents);
    }
}
