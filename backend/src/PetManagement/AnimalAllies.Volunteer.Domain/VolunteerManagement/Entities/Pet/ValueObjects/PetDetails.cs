using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class PetDetails : ValueObject
{
    private PetDetails() { }

    private PetDetails(string description, DateOnly birthDate, DateTime creationTime)
    {
        Description = description;
        BirthDate = birthDate;
        CreationTime = creationTime;
    }

    public string Description { get; }

    public DateOnly BirthDate { get; }

    public DateTime CreationTime { get; }

    public static Result<PetDetails> Create(string description, DateOnly birthDate, DateTime creationTime)
    {
        if (string.IsNullOrWhiteSpace(description) ||
            description.Length > Constraints.MAX_DESCRIPTION_LENGTH)
        {
            return Errors.General.ValueIsRequired(description);
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.Now))
        {
            return Errors.General.ValueIsInvalid(nameof(birthDate));
        }

        return new PetDetails(description, birthDate, creationTime);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Description;
        yield return BirthDate;
        yield return CreationTime;
    }
}