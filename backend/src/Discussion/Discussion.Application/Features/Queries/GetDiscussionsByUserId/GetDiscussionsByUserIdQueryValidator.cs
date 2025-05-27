using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace Discussion.Application.Features.Queries.GetDiscussionsByUserId;

public class GetDiscussionsByUserIdQueryValidator: AbstractValidator<GetDiscussionsByUserIdQuery>
{
    public GetDiscussionsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("user id"));
    }
}