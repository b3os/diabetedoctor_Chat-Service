namespace ChatService.Domain.Abstractions;

public abstract class ValueObject
{
    /// <summary>
    /// Các thành phần dùng để so sánh và hash.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj == null || obj.GetType() != GetType()) return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate(1, (hash, component) => HashCode.Combine(hash, component));
    }
}