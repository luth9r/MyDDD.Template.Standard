namespace MyDDD.Template.Domain.Primitives;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(0, HashCode.Combine);
    }

    private bool ValuesAreEqual(ValueObject other)
    {
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public static bool operator ==(ValueObject? first, ValueObject? second) =>
        first is not null && second is not null && first.Equals(second);

    public static bool operator !=(ValueObject? first, ValueObject? second) => !(first == second);
}
