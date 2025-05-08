using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using VolunteerRequests.Domain.ValueObjects;

namespace VolunteerRequests.Application.Features.Commands.SendRequestForRevision;

public class SendRequestForRevisionCommandValidator : AbstractValidator<SendRequestForRevisionCommand>
{
    public SendRequestForRevisionCommandValidator()
    {
        RuleFor(r => r.VolunteerRequestId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer request id"));

        RuleFor(r => r.AdminId)
            .NotEmpty()
            .WithError(Errors.General.Null("admin id"));

        RuleFor(r => r.RejectionComment)
            .MustBeValueObject(RejectionComment.Create);
    }
}