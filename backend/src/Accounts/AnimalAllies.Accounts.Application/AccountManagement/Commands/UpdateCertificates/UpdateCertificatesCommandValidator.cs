using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateCertificates;

public class UpdateCertificatesCommandValidator: AbstractValidator<UpdateCertificatesCommand>
{
    public UpdateCertificatesCommandValidator()
    {
        RuleForEach(s => s.Certificates)
            .MustBeValueObject(c => Certificate
                .Create(
                    c.Title,
                    c.IssuingOrganization,
                    c.IssueDate,
                    c.ExpirationDate,
                    c.Description));

        RuleFor(s => s.UserId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsInvalid("user id"));
    }
}