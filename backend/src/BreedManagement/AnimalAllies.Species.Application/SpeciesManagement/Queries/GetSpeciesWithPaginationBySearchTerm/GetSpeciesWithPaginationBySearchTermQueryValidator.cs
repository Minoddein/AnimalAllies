using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPaginationBySearchTerm;

public class GetSpeciesWithPaginationBySearchTermQueryValidator: 
    AbstractValidator<GetSpeciesWithPaginationBySearchTermQuery>
{
    public GetSpeciesWithPaginationBySearchTermQueryValidator()
    {
        RuleFor(s => s.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsInvalid("page"));
        
        RuleFor(s => s.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsInvalid("page size"));
    }
}