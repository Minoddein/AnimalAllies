namespace AnimalAllies.Core.DTOs.ValueObjects;

public record MedicalInfoDto(
    bool? IsSpayedNeutered,
    bool? IsVaccinated,
    DateTime? LastVaccinationDate,
    bool? HasChronicDiseases,
    string? MedicalNotes,
    bool? RequiresSpecialDiet,
    bool? HasAllergies);