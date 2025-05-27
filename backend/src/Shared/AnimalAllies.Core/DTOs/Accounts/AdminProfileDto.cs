namespace AnimalAllies.Core.DTOs.Accounts;

public class AdminProfileDto
{
    public Guid AdminUserId {get; set;}
    public Guid AdminId { get; set; }
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminSecondName { get; set; } = string.Empty;
    public string AdminPatronymic { get; set; } = string.Empty;
}