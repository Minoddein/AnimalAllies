using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class Temperament : ValueObject
{
    public int AggressionLevel { get; }
    public int Friendliness { get; }
    public int ActivityLevel { get; }
    public bool GoodWithKids { get; }
    public bool GoodWithPeople { get; }
    public bool GoodWithOtherAnimals { get; }

    private Temperament(
        int aggressionLevel,
        int friendliness,
        int activityLevel,
        bool goodWithKids,
        bool goodWithPeople,
        bool goodWithOtherAnimals)
    {
        AggressionLevel = aggressionLevel;
        Friendliness = friendliness;
        ActivityLevel = activityLevel;
        GoodWithKids = goodWithKids;
        GoodWithPeople = goodWithPeople;
        GoodWithOtherAnimals = goodWithOtherAnimals;
    }

    public static Result<Temperament> Create(
        int aggressionLevel,
        int friendliness,
        int activityLevel,
        bool goodWithKids,
        bool goodWithPeople,
        bool goodWithOtherAnimals)
    {
        if (aggressionLevel is < 1 or > 10)
            return Errors.General.ValueIsInvalid(nameof(aggressionLevel));
        
        if (friendliness is < 1 or > 10)
            return Errors.General.ValueIsInvalid(nameof(friendliness));
        
        if (activityLevel is < 1 or > 10)
            return Errors.General.ValueIsInvalid(nameof(activityLevel));

        return new Temperament(
            aggressionLevel,
            friendliness,
            activityLevel,
            goodWithKids,
            goodWithPeople,
            goodWithOtherAnimals);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AggressionLevel;
        yield return Friendliness;
        yield return ActivityLevel;
        yield return GoodWithKids;
        yield return GoodWithPeople;
        yield return GoodWithOtherAnimals;
    }
}