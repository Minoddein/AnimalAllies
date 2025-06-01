namespace AnimalAllies.Core.DTOs;

public class SpeciesDto
{
    public Guid SpeciesId { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public BreedDto[] Breeds { get; set; } = [];
}