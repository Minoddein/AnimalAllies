using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddCertificates;

public class AddCertificatesCommandValidator: AbstractValidator<AddCertificatesCommand>
{
    public AddCertificatesCommandValidator()
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