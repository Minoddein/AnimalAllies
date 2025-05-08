using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestsInWaitingWithPagination;

public class GetVolunteerRequestsInWaitingWithPaginationQueryValidator :
    AbstractValidator<GetVolunteerRequestsInWaitingWithPaginationQuery>
{
    public GetVolunteerRequestsInWaitingWithPaginationQueryValidator()
    {
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
    }
}