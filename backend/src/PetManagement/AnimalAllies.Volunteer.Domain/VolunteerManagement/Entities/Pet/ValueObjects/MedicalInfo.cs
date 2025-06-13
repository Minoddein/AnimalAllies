using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Objects;

namespace AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;

public class MedicalInfo : ValueObject
{
    public bool? IsSpayedNeutered  { get; }
    public bool? IsVaccinated { get; }
    public DateTime? LastVaccinationDate { get; }
    public bool? HasChronicDiseases { get; }
    public string? MedicalNotes { get; }
    public bool? RequiresSpecialDiet { get; }
    public bool? HasAllergies { get; }

    private MedicalInfo(
        bool? isSpayedNeutered,
        bool? isVaccinated,
        DateTime? lastVaccinationDate,
        bool? hasChronicDiseases,
        string? medicalNotes,
        bool? requiresSpecialDiet,
        bool? hasAllergies)
    {
        IsSpayedNeutered = isSpayedNeutered;
        IsVaccinated = isVaccinated;
        LastVaccinationDate = lastVaccinationDate;
        HasChronicDiseases = hasChronicDiseases;
        MedicalNotes = medicalNotes;
        RequiresSpecialDiet = requiresSpecialDiet;
        HasAllergies = hasAllergies;
    }

    public static Result<MedicalInfo> Create(
        bool isSpayedNeutered,
        bool isVaccinated,
        DateTime? lastVaccinationDate,
        bool hasChronicDiseases,
        string? medicalNotes,
        bool requiresSpecialDiet,
        bool hasAllergies)
    {
        if (medicalNotes?.Length > Constraints.MAX_MEDICAL_NOTES_LENGTH)
            return Errors.General.ValueTooLong(nameof(medicalNotes));

        if (lastVaccinationDate > DateTime.UtcNow)
            return Errors.General.ValueIsInvalid(nameof(lastVaccinationDate));

        return new MedicalInfo(
            isSpayedNeutered,
            isVaccinated,
            lastVaccinationDate,
            hasChronicDiseases,
            medicalNotes,
            requiresSpecialDiet,
            hasAllergies);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsSpayedNeutered ?? false;
        yield return IsVaccinated ?? false;
        yield return LastVaccinationDate ?? DateTime.MinValue;
        yield return HasChronicDiseases ?? false;
        yield return MedicalNotes ?? string.Empty;
        yield return RequiresSpecialDiet ?? false;
        yield return HasAllergies ?? false;
    }
}