using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Queries.GetFilteredVolunteerRequestsByUserIdWithPagination;

public class GetFilteredVolunteerRequestsByUserIdWithPaginationQueryValidator :
    AbstractValidator<GetFilteredVolunteerRequestsByUserIdWithPaginationQuery>
{
    public GetFilteredVolunteerRequestsByUserIdWithPaginationQueryValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));

        RuleFor(v => v.Page)
            .GreaterThan(0)
            .WithError(Errors.General.ValueIsInvalid("page"));

        RuleFor(v => v.PageSize)
            .GreaterThan(0)
            .WithError(Errors.General.ValueIsInvalid("page size"));

        RuleFor(v => v.SortBy)
            .MaximumLength(Constraints.MAX_VALUE_LENGTH)
            .WithError(Errors.General.ValueIsInvalid("sort by"));

        RuleFor(v => v.SortDirection)
            .MaximumLength(Constraints.MAX_VALUE_LENGTH)
            .WithError(Errors.General.ValueIsInvalid("sort direction"));

        RuleFor(V => V.RequestStatus)
            .MaximumLength(Constraints.MAX_VALUE_LENGTH)
            .WithError(Errors.General.ValueIsInvalid("request status"));
    }
}