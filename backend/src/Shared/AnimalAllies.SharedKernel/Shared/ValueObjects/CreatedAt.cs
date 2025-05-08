using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.SharedKernel.Shared.ValueObjects;

public class CreatedAt : ValueObject
{
    private CreatedAt() { }

    private CreatedAt(DateTime value) => Value = value;

    public DateTime Value { get; }

    public static Result<CreatedAt> Create(DateTime value)
    {
        if (value > DateTime.Now)
        {
            return Errors.Errors.General.ValueIsInvalid("created at");
        }

        return new CreatedAt(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}