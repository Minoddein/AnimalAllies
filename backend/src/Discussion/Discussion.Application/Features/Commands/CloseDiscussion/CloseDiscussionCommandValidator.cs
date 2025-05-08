using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace Discussion.Application.Features.Commands.CloseDiscussion;

public class CloseDiscussionCommandValidator : AbstractValidator<CloseDiscussionCommand>
{
    public CloseDiscussionCommandValidator()
    {
        RuleFor(p => p.DiscussionId)
            .NotEmpty()
            .WithError(Errors.General.Null("discussion id"));

        RuleFor(p => p.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
    }
}