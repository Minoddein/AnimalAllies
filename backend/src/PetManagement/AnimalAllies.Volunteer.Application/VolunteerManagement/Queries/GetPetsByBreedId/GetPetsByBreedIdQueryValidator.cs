using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsByBreedId;

public class GetPetsByBreedIdQueryValidator : AbstractValidator<GetPetsByBreedIdQuery>
{
    public GetPetsByBreedIdQueryValidator()
    {
        RuleFor(p => p.BreedId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("breed id"));
        RuleFor(p => p.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsInvalid("page"));

        RuleFor(p => p.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithError(Errors.General.ValueIsInvalid("page size"));
    }
}