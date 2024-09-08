using AnimalAllies.Application.Validators;
using AnimalAllies.Domain.Shared;
using FluentValidation;

namespace AnimalAllies.Application.Features.Volunteer.CreateRequisites;

public class CreateRequisitesValidator: AbstractValidator<CreateRequisitesCommand>
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