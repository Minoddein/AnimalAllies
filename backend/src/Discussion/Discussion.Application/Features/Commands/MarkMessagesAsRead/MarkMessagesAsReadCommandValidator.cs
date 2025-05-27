using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace Discussion.Application.Features.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandValidator: AbstractValidator<MarkMessagesAsReadCommand>
{
    public MarkMessagesAsReadCommandValidator()
    {
        RuleFor(p => p.DiscussionId)
            .NotEmpty()
            .WithError(Errors.General.Null("discussion id"));
                
        RuleFor(p => p.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));
    }
}