using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;

public class IsUserExistByIdQueryValidator : AbstractValidator<IsUserExistByIdQuery>
{
    public IsUserExistByIdQueryValidator() =>
        RuleFor(u => u.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
}