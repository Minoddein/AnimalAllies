using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace VolunteerRequests.Domain.ValueObjects;

public class RejectionComment : ValueObject
{
    private RejectionComment() { }

    private RejectionComment(string value) => Value = value;

    public string Value { get; }

    public static Result<RejectionComment> Create(string value)
    {
        if (string.IsNullOrEmpty(value) && value.Length > Constraints.MAX_DESCRIPTION_LENGTH)
        {
            return Errors.General.ValueIsRequired("rejection comment");
        }

        return new RejectionComment(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}