using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class HelpStatus : ValueObject
{
    public static readonly HelpStatus NeedsHelp = new(nameof(NeedsHelp));
    public static readonly HelpStatus SearchingHome = new(nameof(SearchingHome));
    public static readonly HelpStatus FoundHome = new(nameof(FoundHome));

    private static readonly HelpStatus[] _all = [NeedsHelp, SearchingHome, FoundHome];

    private HelpStatus() { }

    private HelpStatus(string value) => Value = value;

    public string Value { get; }

    public static Result<HelpStatus> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Errors.General.ValueIsRequired(input);
        }

        if (_all.Any(s => s.Value.Equals(input, StringComparison.CurrentCultureIgnoreCase)) == false)
        {
            return Errors.General.ValueIsInvalid(input);
        }

        return new HelpStatus(input);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}