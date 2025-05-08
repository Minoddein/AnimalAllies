using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace VolunteerRequests.Application.Features.Commands.CreateVolunteerRequest;

public class CreateVolunteerRequestValidator : AbstractValidator<CreateVolunteerRequestCommand>
{
    public CreateVolunteerRequestValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithError(Errors.General.Null("user id"));

        RuleFor(v => v.FullNameDto)
            .MustBeValueObject(f => FullName.Create(f.FirstName, f.SecondName, f.Patronymic));

        RuleFor(v => v.Email)
            .MustBeValueObject(Email.Create);

        RuleFor(v => v.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(v => v.WorkExperience)
            .MustBeValueObject(WorkExperience.Create);

        RuleFor(v => v.VolunteerDescription)
            .MustBeValueObject(VolunteerDescription.Create);

        RuleForEach(v => v.SocialNetworkDtos)
            .MustBeValueObject(s => SocialNetwork.Create(s.Title, s.Url));
    }
}