using AnimalAllies.Core.DTOs.ValueObjects;

namespace AnimalAllies.Core.DTOs.Accounts;

public class ParticipantAccountDto
{
    public Guid ParticipantUserId {get; set;}
    public Guid ParticipantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
}