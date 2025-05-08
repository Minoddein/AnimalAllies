using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;

public class CreateRequisitesValidator : AbstractValidator<CreateRequisitesCommand>
{
    public CreateRequisitesValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id cannot be null");

        RuleForEach(x => x.RequisiteDtos)
            .MustBeValueObject(x => Requisite.Create(x.Title, x.Description));
    }
}