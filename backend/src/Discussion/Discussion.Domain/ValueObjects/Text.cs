using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace Discussion.Domain.ValueObjects;

public class Text : ValueObject
{
    private Text() { }

    private Text(string value) => Value = value;

    public string Value { get; set; }

    public static Result<Text> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > Constraints.MAX_DESCRIPTION_LENGTH)
        {
            return Errors.General.ValueIsInvalid("text");
        }

        return new Text(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}