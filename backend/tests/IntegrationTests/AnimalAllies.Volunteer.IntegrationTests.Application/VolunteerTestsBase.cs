using AnimalAllies.Species.Infrastructure.DbContexts;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;

namespace AnimalAllies.Volunteer.IntegrationTests.Application;

public class VolunteerTestsBase : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    private readonly IntegrationTestsWebFactory _factory;
    protected readonly Fixture _fixture;
    protected readonly IServiceScope _scope;
    protected readonly SpeciesWriteDbContext _speciesDbContext;
    protected readonly VolunteerWriteDbContext _volunteerDbContext;

    protected VolunteerTestsBase(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _volunteerDbContext = _scope.ServiceProvider.GetRequiredService<VolunteerWriteDbContext>();
        _speciesDbContext = _scope.ServiceProvider.GetRequiredService<SpeciesWriteDbContext>();
        _fixture = new Fixture();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync().ConfigureAwait(false);

        _scope.Dispose();
    }
}