using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using Discussion.Domain.ValueObjects;
using FluentValidation;

namespace Discussion.Application.Features.Commands.PostMessage;

public class PostMessageCommandValidator : AbstractValidator<PostMessageCommand>
{
    public PostMessageCommandValidator()
    {
        RuleFor(p => p.DiscussionId)
            .NotEmpty()
            .WithError(Errors.General.Null("discussion id"));

        RuleFor(p => p.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));

        RuleFor(p => p.Text)
            .MustBeValueObject(Text.Create);
    }
}