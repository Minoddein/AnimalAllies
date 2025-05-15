using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateRequisites;

public class UpdateRequisitesCommandValidator: AbstractValidator<UpdateRequisitesCommand>
{
    public UpdateRequisitesCommandValidator()
    {
        RuleForEach(r => r.Requisites)
            .MustBeValueObject(r => Requisite
                .Create(r.Title, r.Description));

        RuleFor(s => s.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsInvalid("user id"));
    }
}