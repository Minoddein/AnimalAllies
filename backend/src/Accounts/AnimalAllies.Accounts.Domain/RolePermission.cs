namespace AnimalAllies.Accounts.Domain;

public class RolePermission
{
    public Guid RoleId { get; init; }

    public Role Role { get; init; } = default!;

    public Guid PermissionId { get; init; }

    public Permission Permission { get; init; } = default!;
}