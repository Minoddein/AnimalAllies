using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Aggregate.ValueObject;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateSkills;

public class UpdateSkillsHandler : ICommandHandler<UpdateSkillsCommand>
{
    private readonly ILogger<UpdateSkillsHandler> _logger;
    private readonly IValidator<UpdateSkillsCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVolunteerRepository _repository;

    public UpdateSkillsHandler(
        ILogger<UpdateSkillsHandler> logger,
        IValidator<UpdateSkillsCommand> validator,
        [FromKeyedServices(Constraints.Context.PetManagement)]
        IUnitOfWork unitOfWork,
        IVolunteerRepository repository)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateSkillsCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }
        
        var volunteerId = VolunteerId.Create(command.VolunteerId);

        var volunteer = await _repository.GetById(volunteerId, cancellationToken);
        if (volunteer.IsFailure)
        {
            return volunteer.Errors;
        }

        var skills = command.Skills.Select(s => Skill.Create(s.SkillName).Value);
        var skillsValueObjectList = new ValueObjectList<Skill>(skills);
        
        var result = volunteer.Value.UpdateSkills(skillsValueObjectList);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _repository.Save(volunteer.Value ,cancellationToken);
        
        _logger.LogInformation("Updated Skills for Volunteer {volunteerId}", command.VolunteerId);
        
        return Result.Success();
    }
}