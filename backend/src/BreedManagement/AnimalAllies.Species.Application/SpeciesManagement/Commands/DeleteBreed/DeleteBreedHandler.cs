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
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteBreed;

public class DeleteBreedHandler : ICommandHandler<DeleteBreedCommand, BreedId>
{
    private readonly ISpeciesRepository _repository;
    private readonly IVolunteerContract _volunteerContract;
    private readonly IValidator<DeleteBreedCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteBreedHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public DeleteBreedHandler(
        ISpeciesRepository repository, 
        IValidator<DeleteBreedCommand> validator,
        ILogger<DeleteBreedHandler> logger,
        [FromKeyedServices(Constraints.Context.BreedManagement)]IUnitOfWork unitOfWork,
        IVolunteerContract volunteerContract, 
        IDateTimeProvider dateTimeProvider,
        IPublisher publisher)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _volunteerContract = volunteerContract;
        _dateTimeProvider = dateTimeProvider;
        _publisher = publisher;
    }
    
    
    public async Task<Result<BreedId>> Handle(
        DeleteBreedCommand command, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var breedId = BreedId.Create(command.BreedId);
        var speciesId = SpeciesId.Create(command.SpeciesId);

        var species = await _repository.GetById(speciesId, cancellationToken);
        if (species.IsFailure)
            return Errors.General.NotFound();

        var petOfThisBreed = await _volunteerContract
            .CheckIfPetByBreedIdExist(breedId.Id, cancellationToken);
        
        if (petOfThisBreed)
            return Errors.Species.DeleteConflict();
        
        var result = species.Value.DeleteBreed(breedId);
        if (result.IsFailure)
            return result.Errors;

        await _publisher.PublishDomainEvents(species.Value, cancellationToken);
        
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Deleted breed with id {breedId} from species with id {speciesId}",breedId.Id, speciesId.Id);

        return breedId;
    }
}