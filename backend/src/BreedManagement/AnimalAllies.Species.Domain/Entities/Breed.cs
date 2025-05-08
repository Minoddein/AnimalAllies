using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.Objects;
using AnimalAllies.SharedKernel.Shared.ValueObjects;

namespace AnimalAllies.Species.Domain.Entities;

public class Breed : DomainEntity<BreedId>
{
    private Breed(BreedId id)
        : base(id)
    {
    }

    public Breed(BreedId breedId, Name name)
        : base(breedId) => Name = name;

    public Name Name { get; private set; }
}