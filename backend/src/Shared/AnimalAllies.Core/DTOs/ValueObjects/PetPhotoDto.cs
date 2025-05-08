namespace AnimalAllies.Core.DTOs.ValueObjects;

public class PetPhotoDto
{
    public string Path { get; init; } = string.Empty;

    public bool IsMain { get; set; }
}