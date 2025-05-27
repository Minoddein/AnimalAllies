using AnimalAllies.SharedKernel.Shared.Objects;
using Discussion.Domain.Entities;

namespace Discussion.Domain.ValueObjects;

public class IsRead: ValueObject
{
    public bool Value { get; }
    
    private IsRead(){}

    public IsRead(bool value)
    {
        Value = value;
    }

    public static bool operator ==(IsRead left, bool right)
    {
        if (ReferenceEquals(left, null))
            return false;
            
        return left.Value == right;
    }
    
    public static bool operator !=(IsRead left, bool right)
    {
        return !(left == right);
    }
    
    public static bool operator ==(bool left, IsRead right)
    {
        return right == left;
    }
    
    public static bool operator !=(bool left, IsRead right)
    {
        return !(left == right);
    }
    
    public static implicit operator IsRead(bool value)
    {
        return new IsRead(value);
    }
    
    public static implicit operator bool(IsRead isRead)
    {
        return isRead.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}