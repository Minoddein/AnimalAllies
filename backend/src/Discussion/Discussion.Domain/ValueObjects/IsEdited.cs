using AnimalAllies.SharedKernel.Shared.Objects;

namespace Discussion.Domain.ValueObjects;

public class IsEdited : ValueObject
{
    private IsEdited() { }

    public IsEdited(bool value) => Value = value;

    public bool Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}