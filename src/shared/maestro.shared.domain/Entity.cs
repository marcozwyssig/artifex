namespace Maestro.Shared.Domain;

/// <summary>
/// Base class for all entities in the domain
/// </summary>
public abstract class Entity<TId> where TId : struct
{
    private int? _requestedHashCode;

    protected Entity(TId id)
    {
        Id = id;
    }
    
    public TId Id { get; protected set; }
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity<TId>)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        Entity<TId> item = (Entity<TId>)obj;
        if (item.IsTransient() || IsTransient())
            return false;

        return item.Id.Equals(Id);
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = Id.GetHashCode() ^ 31;

            return _requestedHashCode.Value;
        }
        else
            return base.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (Object.Equals(left, null))
            return Object.Equals(right, null);
        else
            return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    public bool IsTransient()
    {
        return Id.Equals(default(TId));
    }
}
