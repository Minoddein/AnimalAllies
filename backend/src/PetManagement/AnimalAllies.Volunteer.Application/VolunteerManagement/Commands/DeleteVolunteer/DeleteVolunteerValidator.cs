using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;

public class DeleteVolunteerValidator : AbstractValidator<DeleteVolunteerCommand>
{
    public DeleteVolunteerValidator() =>
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("id cannot be empty");
}