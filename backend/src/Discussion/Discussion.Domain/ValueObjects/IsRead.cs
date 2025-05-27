using AnimalAllies.SharedKernel.Shared.Objects;

namespace Discussion.Domain.ValueObjects;

public class IsRead: ValueObject
{
    public bool Value { get; }
    
    private IsRead(){}

    public IsRead(bool value)
    {
        Value = value;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}