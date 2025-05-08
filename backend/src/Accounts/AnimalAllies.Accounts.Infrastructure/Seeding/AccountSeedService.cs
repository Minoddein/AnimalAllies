using System.Text.Json;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Accounts.Infrastructure.IdentityManagers;
using AnimalAllies.Accounts.Infrastructure.Options;
using AnimalAllies.Framework;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AnimalAllies.Accounts.Infrastructure.Seeding;

public class AccountSeedService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    PermissionManager permissionManager,
    RolePermissionManager rolePermissionManager,
    IOptions<AdminOptions> adminOptions,
    ILogger<AccountSeedService> logger,
    AccountManager accountManager)
{
    private readonly AccountManager _accountManager = accountManager;
    private readonly AdminOptions _adminOptions = adminOptions.Value;
    private readonly ILogger<AccountSeedService> _logger = logger;
    private readonly PermissionManager _permissionManager = permissionManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly RolePermissionManager _rolePermissionManager = rolePermissionManager;
    private readonly UserManager<User> _userManager = userManager;

    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding accounts...");

        string json = await File.ReadAllTextAsync(FilePaths.Accounts).ConfigureAwait(false);

        RolePermissionOptions seedData = JsonSerializer.Deserialize<RolePermissionOptions>(json)
                                         ?? throw new ApplicationException(
                                             "Could not deserialize role permission config.");

        await SeedPermissions(seedData).ConfigureAwait(false);

        await SeedRoles(seedData).ConfigureAwait(false);

        await SeedRolePermissions(seedData).ConfigureAwait(false);

        await CreateAdmin().ConfigureAwait(false);
    }

    private async Task CreateAdmin()
    {
        Role adminRole = await _roleManager.FindByNameAsync(AdminProfile.ADMIN).ConfigureAwait(false)
                         ?? throw new ApplicationException("Could not find admin role");

        User adminUser = User.CreateAdmin(_adminOptions.UserName, _adminOptions.Email, adminRole);

        User? isAdminExist = await _userManager.FindByNameAsync(AdminProfile.ADMIN).ConfigureAwait(false);
        if (isAdminExist is not null)
        {
            return;
        }

        adminUser.EmailConfirmed = true;

        await _userManager.CreateAsync(adminUser, _adminOptions.Password).ConfigureAwait(false);

        Result<FullName> fullName = FullName.Create(
            _adminOptions.UserName,
            _adminOptions.UserName,
            _adminOptions.UserName);

        AdminProfile adminProfile = new(fullName.Value, adminUser);

        await _accountManager.CreateAdminAccount(adminProfile).ConfigureAwait(false);
    }

    private async Task SeedRolePermissions(RolePermissionOptions seedData)
    {
        foreach (string roleName in seedData.Roles.Keys)
        {
            Role? role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);

            await _rolePermissionManager.AddRangeIfExist(role!.Id, seedData.Roles[roleName]).ConfigureAwait(false);
        }

        _logger.LogInformation("RolePermission added to database");
    }

    private async Task SeedRoles(RolePermissionOptions seedData)
    {
        foreach (string roleName in seedData.Roles.Keys)
        {
            Role? existingRole = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);

            if (existingRole is null)
            {
                await _roleManager.CreateAsync(new Role { Name = roleName }).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Roles added to database");
    }

    private async Task SeedPermissions(RolePermissionOptions seedData)
    {
        IEnumerable<string> permissionsToAdd = seedData.Permissions
            .SelectMany(permissionGroup => permissionGroup.Value);

        await _permissionManager.AddRangeIfExist(permissionsToAdd).ConfigureAwait(false);

        _logger.LogInformation("Permissions added to database");
    }
}