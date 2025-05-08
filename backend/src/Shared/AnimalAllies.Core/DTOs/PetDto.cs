using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs;

public class PetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public string HealthInformation { get; set; } = string.Empty;

    public double Weight { get; set; }

    public double Height { get; set; }

    public bool IsCastrated { get; set; }

    public bool IsVaccinated { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string HelpStatus { get; set; } = string.Empty;

    public Guid VolunteerId { get; set; }

    public Guid SpeciesId { get; set; }

    public Guid BreedId { get; set; }

    public RequisiteDto[] Requisites { get; set; } = [];

    public PetPhotoDto[] PetPhotos { get; set; } = [];

    public bool IsDeleted { get; set; }
}