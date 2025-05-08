using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.MovePetPosition;

public class MovePetPositionValidator : AbstractValidator<MovePetPositionCommand>
{
    public MovePetPositionValidator()
    {
        RuleFor(p => p.VolunteerId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("VolunteerId"));

        RuleFor(p => p.PetId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("PetId"));

        RuleFor(p => p.Position)
            .MustBeValueObject(p => Position.Create(p.Position));
    }
}