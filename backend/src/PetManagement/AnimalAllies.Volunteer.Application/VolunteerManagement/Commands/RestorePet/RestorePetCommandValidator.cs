using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestorePet;

public class RestorePetCommandValidator : AbstractValidator<RestorePetCommand>
{
    public RestorePetCommandValidator()
    {
        RuleFor(p => p.VolunteerId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer id"));

        RuleFor(p => p.PetId)
            .NotEmpty()
            .WithError(Errors.General.Null("pet id"));
    }
}