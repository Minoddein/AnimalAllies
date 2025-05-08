using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.DeleteRefreshSession;

public class DeleteRefreshTokenValidator : AbstractValidator<DeleteRefreshTokenCommand>
{
    public DeleteRefreshTokenValidator() =>
        RuleFor(r => r.RefreshToken)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("refresh token"));
}