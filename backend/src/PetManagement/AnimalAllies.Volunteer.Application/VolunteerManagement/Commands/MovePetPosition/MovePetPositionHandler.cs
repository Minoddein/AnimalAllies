using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.MovePetPosition;

public class MovePetPositionHandler(
    IVolunteerRepository repository,
    IValidator<MovePetPositionCommand> validator,
    ILogger<MovePetPositionHandler> logger,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork) : ICommandHandler<MovePetPositionCommand, VolunteerId>
{
    private readonly ILogger<MovePetPositionHandler> _logger = logger;
    private readonly IVolunteerRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<MovePetPositionCommand> _validator = validator;

    public async Task<Result<VolunteerId>> Handle(
        MovePetPositionCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        VolunteerId volunteerId = VolunteerId.Create(command.VolunteerId);

        Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteer =
            await _repository.GetById(volunteerId, cancellationToken).ConfigureAwait(false);
        if (volunteer.IsFailure)
        {
            return volunteer.Errors;
        }

        PetId petId = PetId.Create(command.PetId);

        Result<Pet> pet = volunteer.Value.GetPetById(petId);
        if (pet.IsFailure)
        {
            return pet.Errors;
        }

        Position position = Position.Create(command.Position.Position).Value;

        Result moveResult = volunteer.Value.MovePet(pet.Value, position);

        if (moveResult.IsFailure)
        {
            return moveResult.Errors;
        }

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "pet with id {petId} of volunteer with id {volunteerId} move to position {position}",
            petId.Id,
            volunteerId.Id,
            position);

        return volunteerId;
    }
}