using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.SetNotificationSettings;

public class SetNotificationSettingsCommandValidator : AbstractValidator<SetNotificationSettingsCommand>
{
    public SetNotificationSettingsCommandValidator() =>
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("user id"));
}