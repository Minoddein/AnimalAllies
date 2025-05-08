using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Commands.TakeRequestForSubmit;

public class TakeRequestForSubmitCommandValidator : AbstractValidator<TakeRequestForSubmitCommand>
{
    public TakeRequestForSubmitCommandValidator()
    {
        RuleFor(r => r.AdminId)
            .NotEmpty()
            .WithError(Errors.General.Null("admin id"));

        RuleFor(r => r.VolunteerRequestId)
            .NotEmpty()
            .WithError(Errors.General.Null("volunteer request id"));
    }
}