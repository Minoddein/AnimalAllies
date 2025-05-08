using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetPermissionsByUserId;

public class GetPermissionsByUserIdQueryValidator : AbstractValidator<GetPermissionsByUserIdQuery>
{
    public GetPermissionsByUserIdQueryValidator() =>
        RuleFor(u => u.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("user id"));
}