using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class Address : ValueObject
{
    private Address() { }

    private Address(string street, string city, string state, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public string Street { get; }

    public string City { get; }

    public string State { get; }

    public string ZipCode { get; }

    public static Result<Address> Create(string street, string city, string state, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street) || street.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid(
                $"Street cannot be empty or more then {Constraints.MAX_VALUE_LENGTH}.");
        }

        if (string.IsNullOrWhiteSpace(city) || city.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid($"City cannot be empty or more then {Constraints.MAX_VALUE_LENGTH}.");
        }

        if (string.IsNullOrWhiteSpace(state) || state.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid($"State cannot be empty or more then {Constraints.MAX_VALUE_LENGTH}.");
        }

        if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length > Constraints.MAX_VALUE_LENGTH)
        {
            return Errors.General.ValueIsInvalid(
                $"ZipCode cannot be empty or more then {Constraints.MAX_VALUE_LENGTH}.");
        }

        return new Address(street, city, state, zipCode);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
    }
}