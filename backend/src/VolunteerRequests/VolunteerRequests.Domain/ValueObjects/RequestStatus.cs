using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace VolunteerRequests.Domain.ValueObjects;

public class RequestStatus : ValueObject
{
    public static readonly RequestStatus Waiting = new(nameof(Waiting));
    public static readonly RequestStatus Submitted = new(nameof(Submitted));
    public static readonly RequestStatus Rejected = new(nameof(Rejected));
    public static readonly RequestStatus RevisionRequired = new(nameof(RevisionRequired));
    public static readonly RequestStatus Approved = new(nameof(Approved));

    private static readonly RequestStatus[] _all = [Submitted, Rejected, RevisionRequired, Approved];

    private RequestStatus() { }

    private RequestStatus(string value) => Value = value;

    public string Value { get; }

    public static Result<RequestStatus> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Errors.General.ValueIsRequired(value);
        }

        if (_all.Any(s => s.Value.Equals(value, StringComparison.CurrentCultureIgnoreCase)) == false)
        {
            return Errors.General.ValueIsInvalid(value);
        }

        return new RequestStatus(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}