using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetAllFilteredPetsWithPagination;

public class GetAllFilteredPetsWithPaginationQueryValidator : AbstractValidator<GetAllFilteredPetsWithPaginationQuery>
{
    public GetAllFilteredPetsWithPaginationQueryValidator()
    {
        RuleFor(v => v.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsRequired("page"));
        
        RuleFor(v => v.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsRequired("page size"));
    }
}
