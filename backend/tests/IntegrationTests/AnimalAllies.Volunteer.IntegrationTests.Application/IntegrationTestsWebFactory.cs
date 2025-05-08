using System.Data.Common;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Species.Contracts;
using AnimalAllies.Species.Infrastructure.DbContexts;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NSubstitute;
using Respawn;
using Testcontainers.PostgreSql;

namespace AnimalAllies.Volunteer.IntegrationTests.Application;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("animalAllies_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly ISpeciesContracts _speciesContractMock = Substitute.For<ISpeciesContracts>();
    private DbConnection _dbConnection = null!;

    private Respawner _respawner = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync().ConfigureAwait(false);

        using IServiceScope scope = Services.CreateScope();

        VolunteerWriteDbContext volunteerDbContext =
            scope.ServiceProvider.GetRequiredService<VolunteerWriteDbContext>();
        await volunteerDbContext.Database.MigrateAsync().ConfigureAwait(false);
        SpeciesWriteDbContext speciesDbContext = scope.ServiceProvider.GetRequiredService<SpeciesWriteDbContext>();
        await speciesDbContext.Database.MigrateAsync().ConfigureAwait(false);

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await InitializeRespawner().ConfigureAwait(false);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync().ConfigureAwait(false);
        await _dbContainer.DisposeAsync().ConfigureAwait(false);

        await base.DisposeAsync().ConfigureAwait(false);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ADMIN__USERNAME", "Admin");
        Environment.SetEnvironmentVariable("ADMIN__EMAIL", "admin@gmail.com");
        Environment.SetEnvironmentVariable("ADMIN__PASSWORD", "Admin123");

        builder.ConfigureTestServices(ConfigureDefaultServices);
    }

    protected virtual void ConfigureDefaultServices(IServiceCollection services)
    {
        services.RemoveAll<IHostedService>();

        services.RemoveAll<VolunteerWriteDbContext>();

        services.RemoveAll<SpeciesWriteDbContext>();

        string? connectionString = _dbContainer.GetConnectionString();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:DefaultConnection", connectionString }
                }
                !)
            .Build();

        services.AddScoped<VolunteerWriteDbContext>(_ =>
            new VolunteerWriteDbContext(configuration));

        services.AddScoped<SpeciesWriteDbContext>(_ =>
            new SpeciesWriteDbContext(configuration));

        services.RemoveAll<ISpeciesContracts>();
        services.AddScoped<ISpeciesContracts>(_ => _speciesContractMock);
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync().ConfigureAwait(false);
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["volunteers", "accounts", "species", "volunteer_requests", "discussions"]
            }).ConfigureAwait(false);
    }

    public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_dbConnection).ConfigureAwait(false);

    public void SetupSuccessSpeciesContractsMock(Guid speciesId, Guid breedId)
    {
        SpeciesDto speciesDto = new() { Id = speciesId, Name = "Dog" };

        BreedDto breedDto = new() { Id = breedId, Name = "Labrador" };

        _speciesContractMock.GetSpecies(Arg.Any<CancellationToken>())
            .Returns([speciesDto.Id]);

        _speciesContractMock.GetBreedsBySpeciesId(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([breedDto.Id]);
    }
}