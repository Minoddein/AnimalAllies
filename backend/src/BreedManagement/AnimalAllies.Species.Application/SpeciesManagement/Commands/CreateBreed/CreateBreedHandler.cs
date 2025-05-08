using System.Transactions;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Application.Repository;
using AnimalAllies.Species.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateBreed;

public class CreateBreedHandler(
    ISpeciesRepository repository,
    IValidator<CreateBreedCommand> validator,
    ILogger<CreateBreedHandler> logger,
    [FromKeyedServices(Constraints.Context.BreedManagement)]
    IUnitOfWork unitOfWork,
    IPublisher publisher) : ICommandHandler<CreateBreedCommand, BreedId>
{
    private readonly ILogger<CreateBreedHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly ISpeciesRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateBreedCommand> _validator = validator;

    public async Task<Result<BreedId>> Handle(CreateBreedCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            SpeciesId speciesId = SpeciesId.Create(command.SpeciesId);

            Result<Domain.Species> species =
                await _repository.GetById(speciesId, cancellationToken).ConfigureAwait(false);
            if (species.IsFailure)
            {
                return Errors.General.NotFound();
            }

            BreedId breedId = BreedId.NewGuid();
            Name name = Name.Create(command.Name).Value;

            Breed breed = new(breedId, name);

            species.Value.AddBreed(breed);

            _repository.Save(species.Value, cancellationToken);

            await _publisher.PublishDomainEvents(species.Value, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("Breed with id {breedId} created to species with id {speciesId}", breedId.Id,
                speciesId.Id);

            return breedId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Breed creation Failed");

            return Error.Failure("fail.to.create.breed", "Fail to create breed");
        }
    }
}