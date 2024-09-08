namespace AnimalAllies.Application.Contracts.DTOs.ValueObjects;

public record PetPhysicCharacteristicsDto(
    string Color,
    string HealthInformation,
    double Weight,
    double Height,
    bool IsCastrated,
    bool IsVaccinated);
