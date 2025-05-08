using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Species.Application.Repository;
using AnimalAllies.Volunteer.Contracts;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteBreed;

public class DeleteBreedHandler(
    ISpeciesRepository repository,
    IValidator<DeleteBreedCommand> validator,
    ILogger<DeleteBreedHandler> logger,
    [FromKeyedServices(Constraints.Context.BreedManagement)]
    IUnitOfWork unitOfWork,
    IVolunteerContract volunteerContract,
    IDateTimeProvider dateTimeProvider,
    IPublisher publisher) : ICommandHandler<DeleteBreedCommand, BreedId>
{
    private readonly ILogger<DeleteBreedHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly ISpeciesRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeleteBreedCommand> _validator = validator;
    private readonly IVolunteerContract _volunteerContract = volunteerContract;

    public async Task<Result<BreedId>> Handle(
        DeleteBreedCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        BreedId breedId = BreedId.Create(command.BreedId);
        SpeciesId speciesId = SpeciesId.Create(command.SpeciesId);

        Result<Domain.Species> species = await _repository.GetById(speciesId, cancellationToken).ConfigureAwait(false);
        if (species.IsFailure)
        {
            return Errors.General.NotFound();
        }

        bool petOfThisBreed = await _volunteerContract
            .CheckIfPetByBreedIdExist(breedId.Id, cancellationToken).ConfigureAwait(false);

        if (petOfThisBreed)
        {
            return Errors.Species.DeleteConflict();
        }

        Result result = species.Value.DeleteBreed(breedId);
        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _publisher.PublishDomainEvents(species.Value, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted breed with id {breedId} from species with id {speciesId}", breedId.Id,
            speciesId.Id);

        return breedId;
    }
}