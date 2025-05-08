namespace AnimalAllies.Core.DTOs.Accounts;

public class ParticipantAccountDto
{
    public Guid ParticipantId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string SecondName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;
}