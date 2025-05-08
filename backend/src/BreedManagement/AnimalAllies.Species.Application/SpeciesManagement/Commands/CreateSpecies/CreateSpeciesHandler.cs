using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Application.Repository;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateSpecies;

public class CreateSpeciesHandler(
    ISpeciesRepository repository,
    IValidator<CreateSpeciesCommand> validator,
    ILogger<CreateSpeciesHandler> logger,
    [FromKeyedServices(Constraints.Context.BreedManagement)]
    IUnitOfWork unitOfWork,
    IPublisher publisher) : ICommandHandler<CreateSpeciesCommand, SpeciesId>
{
    private readonly ILogger<CreateSpeciesHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly ISpeciesRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateSpeciesCommand> _validator = validator;

    public async Task<Result<SpeciesId>> Handle(
        CreateSpeciesCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        SpeciesId speciesId = SpeciesId.NewGuid();
        Name name = Name.Create(command.Name).Value;

        Domain.Species species = new(speciesId, name);

        Result<SpeciesId> result = await _repository.Create(species, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors;
        }

        await _publisher.PublishDomainEvents(species, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Created species with id {speciesId}", speciesId.Id);

        return result.Value;
    }
}