using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;

public class UpdateVolunteerValidator : AbstractValidator<UpdateVolunteerCommand>
{
    public UpdateVolunteerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Dto.FullName)
            .MustBeValueObject(x => FullName.Create(x.FirstName, x.SecondName, x.Patronymic));

        RuleFor(x => x.Dto.Description)
            .MustBeValueObject(VolunteerDescription.Create);

        RuleFor(x => x.Dto.WorkExperience)
            .MustBeValueObject(WorkExperience.Create);

        RuleFor(x => x.Dto.PhoneNumber)
            .MustBeValueObject(PhoneNumber.Create);

        RuleFor(x => x.Dto.Email)
            .MustBeValueObject(Email.Create);
    }
}