using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetPhoto;

public class DeletePetPhotosCommandValidator : AbstractValidator<DeletePetPhotosCommand>
{
    public DeletePetPhotosCommandValidator()
    {
        RuleFor(p => p.PetId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("PetId"));

        RuleFor(p => p.VolunteerId)
            .NotEmpty().WithError(Errors.General.ValueIsRequired("VolunteerId"));

        RuleForEach(p => p.FilePaths)
            .ChildRules(p =>
            {
                p.RuleFor(p => p)
                    .Must(ext => Constraints.Extensions.Contains(Path.GetExtension(ext)))
                    .NotEmpty().WithError(Error.Null(
                        "filename.is.null",
                        "filename cannot be null or empty"));
            });
    }
}