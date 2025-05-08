using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace Discussion.Application.Features.Commands.CreateDiscussion;

public class CreateDiscussionCommandValidator : AbstractValidator<CreateDiscussionCommand>
{
    public CreateDiscussionCommandValidator()
    {
        RuleFor(d => d.FirstMember)
            .NotEmpty()
            .WithError(Errors.General.Null("first member"));

        RuleFor(d => d.SecondMember)
            .NotEmpty()
            .WithError(Errors.General.Null("second member"));

        RuleFor(d => d.RelationId)
            .NotEmpty()
            .WithError(Errors.General.Null("relation id"));
    }
}