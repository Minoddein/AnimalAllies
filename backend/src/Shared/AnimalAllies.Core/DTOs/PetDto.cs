using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs;

public class PetDto
{
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string HealthInformation { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Height { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string HelpStatus { get; set; } = string.Empty;
    public Guid VolunteerId { get; set; }
    public Guid PetSpeciesId { get; set; }
    public string SpeciesName { get; set; } = string.Empty;
    public Guid PetBreedId { get; set; }
    public string BreedName { get; set; } = string.Empty;
    public RequisiteDto[] Requisites { get; set; } = [];
    public PetPhotoDto[] PetPhotos { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime? ArriveDate { get; set; }
    public string? LastOwner { get; set; }
    public string? From { get; set; } 
    public string? Sex { get; set; } 
    public bool? IsSpayedNeutered { get; set; }
    public bool? IsVaccinated { get; set; }
    public DateTime? LastVaccinationDate { get; set; }
    public bool? HasChronicDiseases { get; set; }
    public string? MedicalNotes { get; set; }
    public bool? RequiresSpecialDiet { get; set; }
    public bool? HasAllergies { get; set; }
    public int? AggressionLevel { get; set; } 
    public int? Friendliness { get; set; } 
    public int? ActivityLevel { get; set; }
    public bool? GoodWithKids { get; set; }
    public bool? GoodWithPeople { get; set; }
    public bool? GoodWithOtherAnimals { get; set; }
}