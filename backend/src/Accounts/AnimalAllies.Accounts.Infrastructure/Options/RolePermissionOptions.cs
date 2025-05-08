namespace AnimalAllies.Accounts.Infrastructure.Options;

public class RolePermissionOptions
{
    public Dictionary<string, string[]> Permissions { get; set; } = [];

    public Dictionary<string, string[]> Roles { get; set; } = [];
}