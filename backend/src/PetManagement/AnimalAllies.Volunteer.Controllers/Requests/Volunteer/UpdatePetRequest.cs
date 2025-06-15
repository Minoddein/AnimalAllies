﻿using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;

namespace AnimalAllies.Volunteer.Presentation.Requests.Volunteer;

public record UpdatePetRequest(
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
    public UpdatePetCommand ToCommand(Guid volunteerId,Guid petId)
        => new(volunteerId,
            petId,
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