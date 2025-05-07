using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Application.Repository;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateSpecies;

public class CreateSpeciesHandler : ICommandHandler<CreateSpeciesCommand, SpeciesId>
{
    private readonly ISpeciesRepository _repository;
    private readonly IValidator<CreateSpeciesCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSpeciesHandler> _logger;
    private readonly IPublisher _publisher;

    public CreateSpeciesHandler(
        ISpeciesRepository repository, 
        IValidator<CreateSpeciesCommand> validator,
        ILogger<CreateSpeciesHandler> logger,
        [FromKeyedServices(Constraints.Context.BreedManagement)]IUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }


    public async Task<Result<SpeciesId>> Handle(CreateSpeciesCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var speciesId = SpeciesId.NewGuid();
        var name = Name.Create(command.Name).Value;
        
        var species = new Domain.Species(speciesId, name);

        var result = await _repository.Create(species, cancellationToken);
        
        if (result.IsFailure)
            return result.Errors;
        
        await _publisher.PublishDomainEvents(species, cancellationToken);
        
        await _unitOfWork.SaveChanges(cancellationToken);
        
        _logger.LogInformation("Created species with id {speciesId}", speciesId.Id);
        
        return result.Value;
    }
}