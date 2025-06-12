using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;

public class Skill: SharedKernel.Shared.Objects.ValueObject
{
    public string Value { get; }
    
    private Skill(string value) => Value = value;

    public static Result<Skill> Create(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill) || skill.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueTooLong(skill);
        }

        return new Skill(skill);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}