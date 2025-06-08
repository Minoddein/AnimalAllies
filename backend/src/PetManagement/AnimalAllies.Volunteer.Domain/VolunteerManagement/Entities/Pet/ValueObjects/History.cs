using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class History : ValueObject
{
    public static readonly string Homeless = "Homeless";
    public static readonly string Shelter = "Shelter";
    public static readonly string PrivatePerson = "PrivatePerson";
    
    private static readonly string[] _validSources = [Homeless, Shelter, PrivatePerson];

    public DateTime ArriveDate { get; }
    public string? LastOwner { get; }
    public string From { get; }

    private History(DateTime arriveDate, string? lastOwner, string from)
    {
        ArriveDate = arriveDate;
        LastOwner = lastOwner;
        From = from;
    }

    public static Result<History> Create(DateTime arriveDate, string? lastOwner, string from)
    {
        if (arriveDate > DateTime.UtcNow)
        {
            return Errors.General.ValueIsInvalid(nameof(arriveDate));
        }
        
        if (string.IsNullOrWhiteSpace(from))
        {
            return Errors.General.ValueIsRequired(nameof(from));
        }

        if (!_validSources.Contains(from))
        {
            return Errors.General.ValueIsInvalid(nameof(from));
        }

        if (lastOwner == null) return new History(arriveDate, lastOwner, from);
        
        if (string.IsNullOrWhiteSpace(lastOwner))
        {
            return Errors.General.ValueIsInvalid(nameof(lastOwner));
        }

        if (lastOwner.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueTooLong(nameof(lastOwner));
        }

        return new History(arriveDate, lastOwner, from);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ArriveDate;
        yield return LastOwner ?? string.Empty;
        yield return From;
    }
}