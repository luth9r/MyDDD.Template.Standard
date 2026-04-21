namespace MyDDD.Template.Domain.Primitives;

public abstract class Entity(Guid id) : IEquatable<Entity>
{
    public Guid Id { get; } = id;

    public static bool operator ==(Entity? first, Entity? second)
    {
        if (first is null && second is null)
        {
            return true;
        }

        if (first is null || second is null)
        {
            return false;
        }

        return first.Equals(second);
    }

    public static bool operator !=(Entity? first, Entity? second) => !(first == second);

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Entity)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
