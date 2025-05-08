using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Species.Application.Repository;
using AnimalAllies.Species.Domain.DomainEvents;
using AnimalAllies.Volunteer.Contracts;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteSpecies;

public class DeleteSpeciesHandler(
    ISpeciesRepository repository,
    IValidator<DeleteSpeciesCommand> validator,
    ILogger<DeleteSpeciesHandler> logger,
    [FromKeyedServices(Constraints.Context.BreedManagement)]
    IUnitOfWork unitOfWork,
    IVolunteerContract volunteerContract,
    IPublisher publisher) : ICommandHandler<DeleteSpeciesCommand, SpeciesId>
{
    private readonly ILogger<DeleteSpeciesHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly ISpeciesRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeleteSpeciesCommand> _validator = validator;
    private readonly IVolunteerContract _volunteerContract = volunteerContract;

    public async Task<Result<SpeciesId>> Handle(
        DeleteSpeciesCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        SpeciesId speciesId = SpeciesId.Create(command.SpeciesId);

        Result<Domain.Species> species = await _repository.GetById(speciesId, cancellationToken).ConfigureAwait(false);
        if (species.IsFailure)
        {
            return Errors.General.NotFound();
        }

        bool petOfThisSpecies = await _volunteerContract
            .CheckIfPetBySpeciesIdExist(command.SpeciesId, cancellationToken).ConfigureAwait(false);

        if (petOfThisSpecies)
        {
            return Errors.Species.DeleteConflict();
        }

        Result<SpeciesId> result = _repository.Delete(species.Value, cancellationToken);
        if (result.IsFailure)
        {
            return Error.Failure("delete.species.failure", "species deletion failed");
        }

        SpeciesDeletedDomainEvent @event = new(species.Value.Id.Id);

        await _publisher.PublishDomainEvent(@event, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Species with id {speciesId} has been deleted", speciesId.Id);

        return speciesId;
    }
}