using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;

public class UpdatePetCommandValidator: AbstractValidator<UpdatePetCommand>
{
    public UpdatePetCommandValidator()
    {
        RuleFor(p => p.VolunteerId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("volunteer id"));
        
        RuleFor(p => p.PetId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("pet id"));
        
        RuleFor(p => p.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(p => p.HelpStatus)
            .MustBeValueObject(HelpStatus.Create);

        RuleFor(p => p.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

    }
}