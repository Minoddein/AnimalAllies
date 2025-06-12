using AnimalAllies.Core.Validators;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentValidation;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateSkills;

public class UpdateSkillsCommandValidator: AbstractValidator<UpdateSkillsCommand>
{
    public UpdateSkillsCommandValidator()
    {
        RuleFor(c => c.VolunteerId)
            .NotEmpty()
            .WithError(Errors.General.ValueIsRequired("volunteer_id"));
        
        RuleForEach(c => c.Skills)
            .ChildRules(r =>
            {
                r.RuleFor(i => i.SkillName)
                    .MustBeValueObject(Skill.Create);
            });
    }
}