namespace AnimalAllies.Core.DTOs.Accounts;

public class RoleDto
{
    public Guid RoleId { get; set; }

    public required string Name { get; set; }
}