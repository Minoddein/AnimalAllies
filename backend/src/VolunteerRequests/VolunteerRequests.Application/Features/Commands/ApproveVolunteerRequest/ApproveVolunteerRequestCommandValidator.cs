using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Commands.ApproveVolunteerRequest;

public class ApproveVolunteerRequestCommandValidator : AbstractValidator<ApproveVolunteerRequestCommand>
{
    public ApproveVolunteerRequestCommandValidator()
    {
        RuleFor(a => a.AdminId)
            .NotEmpty()
            .WithError(Errors.General.Null("admin id"));

        RuleFor(a => a.VolunteerRequestId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer request id"));
    }
}