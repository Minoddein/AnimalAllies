using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Queries.GetVolunteerRequestByRelationUser;

public class GetVolunteerRequestByRelationUserQueryValidator: AbstractValidator<GetVolunteerRequestByRelationUserQuery>
{
    public GetVolunteerRequestByRelationUserQueryValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
    }
}