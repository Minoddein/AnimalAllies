using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Species.Domain.DomainEvents;
using AnimalAllies.Species.Domain.Entities;

namespace AnimalAllies.Species.Domain;

public class Species: DomainEntity<SpeciesId>
{
    private readonly List<Breed> _breeds = [];
    private Species(SpeciesId id): base(id){}
    public Species(SpeciesId speciesId,Name name)
        : base(speciesId)
    {
        Name = name;

        var @event = new SpeciesCreatedDomainEvent(speciesId.Id);
        
        AddDomainEvent(@event);
    }
    
    public Name Name { get; private set; }
    public IReadOnlyList<Breed> Breeds => _breeds;

    public Result AddBreed(Breed breed)
    {
        var breedAlreadyExist = _breeds.FirstOrDefault(b => b.Name == breed.Name);
        if (breedAlreadyExist is not null)
            return Errors.Species.BreedAlreadyExist();
        
        _breeds.Add(breed);

        var @event = new BreedCreatedDomainEvent(Id.Id, breed.Id.Id);
        
        AddDomainEvent(@event);

        return Result.Success();
    }

    public Result<Breed> GetById(BreedId id)
    {
        var breed = _breeds.FirstOrDefault(b => b.Id == id);

        if (breed == null)
            return Errors.General.NotFound();

        return breed;
    }

    public Result DeleteBreed(BreedId id, DateTime deletionTime)
    {
        var breed = GetById(id);
        if (breed.IsFailure)
            return Errors.General.NotFound();

        _breeds.Remove(breed.Value);

        var @event = new BreedDeletedDomainEvent(Id.Id ,id.Id);
        
        AddDomainEvent(@event);
        
        return Result.Success();
    }
    
}