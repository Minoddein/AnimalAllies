using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;

public class AddPetCommandValidator : AbstractValidator<AddPetCommand>
{
    public AddPetCommandValidator()
    {
        RuleFor(p => p.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(p => p.HelpStatus)
            .MustBeValueObject(HelpStatus.Create);

        RuleFor(p => p.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(p => p.Address)
            .MustBeValueObject(p => Address.Create(p.Street, p.City, p.State, p.ZipCode));

        RuleFor(p => p.PetDetails.Description)
            .NotEmpty().WithError(Errors.General.ValueIsInvalid(nameof(PetDetails.Description)));

        RuleFor(p => p.PetPhysicCharacteristics)
            .MustBeValueObject(p => PetPhysicCharacteristics.Create(
                p.Color,
                p.HealthInformation,
                p.Weight,
                p.Height));

        RuleFor(p => p.AnimalSex)
            .MustBeValueObject(AnimalSex.Create);

        RuleFor(p => p.History)
            .MustBeValueObject(h => History.Create(h.ArriveTime, h.LastOwner, h.From));

        RuleForEach(x => x.Requisites)
            .MustBeValueObject(x => Requisite.Create(x.Title, x.Description));
    }
}