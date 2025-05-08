using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.DTOs.ValueObjects;

namespace VolunteerRequests.Application.Features.Commands.CreateVolunteerRequest;

public record CreateVolunteerRequestCommand(
    Guid UserId,
    FullNameDto FullNameDto,
    string Email,
    string PhoneNumber,
    int WorkExperience,
    string VolunteerDescription,
    IEnumerable<SocialNetworkDto> SocialNetworkDtos) : ICommand;