namespace AnimalAllies.Core.DTOs;

public class BreedDto
{
    public Guid BreedId { get; init; }
    public Guid SpeciesId { get; init; }
    public string BreedName { get; init; } = string.Empty;
}