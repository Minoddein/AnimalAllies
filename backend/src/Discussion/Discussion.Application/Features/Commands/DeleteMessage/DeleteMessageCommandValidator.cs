using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace Discussion.Application.Features.Commands.DeleteMessage;

public class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(p => p.DiscussionId)
            .NotEmpty()
            .WithError(Errors.General.Null("discussion id"));

        RuleFor(p => p.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));

        RuleFor(p => p.MessageId)
            .NotEmpty()
            .WithError(Errors.General.Null("message id"));
    }
}