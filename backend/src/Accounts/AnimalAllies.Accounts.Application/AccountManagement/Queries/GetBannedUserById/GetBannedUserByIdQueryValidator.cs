using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetBannedUserById;

public class GetBannedUserByIdQueryValidator : AbstractValidator<GetBannedUserByIdQuery>
{
    public GetBannedUserByIdQueryValidator() =>
        RuleFor(b => b.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
}