using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class AnimalSex : ValueObject
{
    public static readonly AnimalSex Male = new("Male");
    public static readonly AnimalSex Female = new("Female");
    public static readonly AnimalSex Unknown = new("Unknown");

    private static readonly AnimalSex[] _all = [Male, Female, Unknown];
    
    public string Value { get; }

    private AnimalSex(string value) => Value = value;

    public static Result<AnimalSex> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Errors.General.ValueIsRequired(nameof(input));

        var sex = _all.FirstOrDefault(s => s.Value.Equals(input, StringComparison.OrdinalIgnoreCase));

        if (sex is null)
        {
            return Errors.General.ValueIsInvalid(nameof(input));
        }
        
        return sex;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}