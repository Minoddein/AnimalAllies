using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace Discussion.Domain.ValueObjects;

public class DiscussionStatus : ValueObject
{
    public static readonly DiscussionStatus Open = new(nameof(Open));
    public static readonly DiscussionStatus Closed = new(nameof(Closed));

    private static readonly DiscussionStatus[] _all = [Open, Closed];

    private DiscussionStatus() { }

    private DiscussionStatus(string value) => Value = value;

    public string Value { get; }

    public static Result<DiscussionStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.General.ValueIsRequired(value);
        }

        if (_all.Any(s => s.Value.Equals(value, StringComparison.CurrentCultureIgnoreCase)) == false)
        {
            return Errors.General.ValueIsInvalid(value);
        }

        return new DiscussionStatus(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}