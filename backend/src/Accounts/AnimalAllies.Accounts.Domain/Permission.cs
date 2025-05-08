namespace AnimalAllies.Accounts.Domain;

public class Permission
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public List<RolePermission> RolePermissions { get; set; } = [];
}