using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.SetMainPhotoOfPet;

public class SetMainPhotoOfPetCommandValidator : AbstractValidator<SetMainPhotoOfPetCommand>
{
    public SetMainPhotoOfPetCommandValidator()
    {
        RuleFor(p => p.PetId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("PetId"));

        RuleFor(p => p.VolunteerId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("VolunteerId"));

        RuleFor(p => p.Path)
            .Must(p => Constraints.Extensions.Contains(Path.GetExtension(p))
                       && Path.GetFileNameWithoutExtension(p).Length > 0);
    }
}