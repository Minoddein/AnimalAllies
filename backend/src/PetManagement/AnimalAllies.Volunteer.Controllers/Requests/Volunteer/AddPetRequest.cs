using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record AddPetRequest(
    string Name,
    PetPhysicCharacteristicsDto PetPhysicCharacteristicsDto,
    PetDetailsDto PetDetailsDto,
    AddressDto AddressDto,
    string PhoneNumber,
    string HelpStatus,
    AnimalTypeDto AnimalTypeDto,
    string AnimalSex,
    HistoryDto HistoryDto,
    TemperamentDto? TemperamentDto,
    MedicalInfoDto? MedicalInfoDto,
    IEnumerable<RequisiteDto> RequisitesDto)
{
    public AddPetCommand ToCommand(Guid volunteerId)
        => new(volunteerId,
            Name,
            PetPhysicCharacteristicsDto,
            PetDetailsDto,
            AddressDto,
            PhoneNumber,
            HelpStatus,
            AnimalTypeDto,
            AnimalSex,
            HistoryDto,
            TemperamentDto,
            MedicalInfoDto,
            RequisitesDto
        );
}
    
    