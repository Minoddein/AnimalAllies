using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Accounts.Infrastructure.Seeding;

public class AccountsSeeder(IServiceScopeFactory serviceScopeFactory)
{
    public async Task SeedAsync()
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        AccountSeedService service = scope.ServiceProvider.GetRequiredService<AccountSeedService>();

        await service.SeedAsync().ConfigureAwait(false);
    }
}