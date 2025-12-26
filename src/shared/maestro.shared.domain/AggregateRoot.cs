namespace Maestro.Shared.Domain;

/// <summary>
/// Base class for aggregate roots
/// Aggregates are clusters of domain objects that can be treated as a single unit
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : struct
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id)
    {
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(DomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
