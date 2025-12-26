namespace Maestro.Shared.Domain;

/// <summary>
/// Base class for all domain events
/// Domain events represent something that happened in the domain that domain experts care about
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; private init; }
    public Guid AggregateId { get; protected init; }
    public string AggregateType { get; protected init; } = string.Empty;
    public long Version { get; protected init; }
    public Guid NodeId { get; protected init; }
    public DateTime OccurredAt { get; private init; }
    public string EventType { get; private init; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = GetType().Name;
    }
}
