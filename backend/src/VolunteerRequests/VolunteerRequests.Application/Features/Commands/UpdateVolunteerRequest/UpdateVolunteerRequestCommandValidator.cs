using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Commands.UpdateVolunteerRequest;

public class UpdateVolunteerRequestCommandValidator: AbstractValidator<UpdateVolunteerRequestCommand>
{
    public UpdateVolunteerRequestCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
        
        RuleFor(v => v.VolunteerRequestId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer request id"));
    }
}