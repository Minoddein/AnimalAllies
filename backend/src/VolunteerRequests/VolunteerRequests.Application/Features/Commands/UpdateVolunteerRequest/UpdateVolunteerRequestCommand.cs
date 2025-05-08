using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace VolunteerRequests.Application.Features.Commands.UpdateVolunteerRequest;

public record UpdateVolunteerRequestCommand(
    Guid UserId,
    Guid VolunteerRequestId,
    FullNameDto FullNameDto,
    string Email,
    string PhoneNumber,
    int WorkExperience,
    string VolunteerDescription,
    IEnumerable<SocialNetworkDto> SocialNetworkDtos) : ICommand;