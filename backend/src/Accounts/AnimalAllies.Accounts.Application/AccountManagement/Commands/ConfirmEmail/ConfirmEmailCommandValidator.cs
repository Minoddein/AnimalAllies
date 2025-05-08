using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(e => e.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsInvalid("user id"));

        RuleFor(e => e.Code)
            .NotEmpty()
            .WithError(Error.Null("code.is.null", "code cannot be null or empty"));
    }
}