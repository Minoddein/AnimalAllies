using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Commands.ResendVolunteerRequest;

public class ResendVolunteerRequestCommandValidator : AbstractValidator<ResendVolunteerRequestCommand>
{
    public ResendVolunteerRequestCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));

        RuleFor(v => v.VolunteerRequestId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer request id"));
    }
}