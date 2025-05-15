﻿using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateSocialNetworks;

public class UpdateSocialNetworkCommandValidator: AbstractValidator<UpdateSocialNetworkCommand>
{
    public UpdateSocialNetworkCommandValidator()
    {
        RuleForEach(s => s.SocialNetworkDtos)
            .MustBeValueObject(sn => SocialNetwork.Create(sn.Title, sn.Url));

        RuleFor(s => s.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsInvalid("user id"));
    }
}