using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;

public record AddPetCommand(
    Guid VolunteerId,
    string Name,
    PetPhysicCharacteristicsDto PetPhysicCharacteristics,
    PetDetailsDto PetDetails,
    AddressDto Address,
    string PhoneNumber,
    string HelpStatus,
    AnimalTypeDto AnimalType,
    IEnumerable<RequisiteDto> Requisites) : ICommand;