using System.Data.Common;
using AnimalAllies.Core.DTOs;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.Species.Contracts;
using AnimalAllies.Species.Infrastructure.DbContexts;
using AnimalAllies.Volunteer.Infrastructure.DbContexts;
using AnimalAllies.Web;
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

public class IntegrationTestsWebFactory: WebApplicationFactory<Program>,IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("animalAllies_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;
    private readonly ISpeciesContracts _speciesContractMock = Substitute.For<ISpeciesContracts>();

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
        

        var connectionString = _dbContainer.GetConnectionString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            }!)
            .Build();
        
        services.AddScoped<VolunteerWriteDbContext>(_ =>
            new VolunteerWriteDbContext(configuration));
        
        services.AddScoped<SpeciesWriteDbContext>(_ =>
            new SpeciesWriteDbContext(configuration));
        
        services.RemoveAll<ISpeciesContracts>();
        services.AddScoped<ISpeciesContracts>(_ => _speciesContractMock);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        using var scope = Services.CreateScope();
        
        var volunteerDbContext = scope.ServiceProvider.GetRequiredService<VolunteerWriteDbContext>();
        await volunteerDbContext.Database.MigrateAsync();
        var speciesDbContext = scope.ServiceProvider.GetRequiredService<SpeciesWriteDbContext>();
        await speciesDbContext.Database.MigrateAsync();
        
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["volunteers", "accounts", "species", "volunteer_requests", "discussions"],
                WithReseed = true
            }
        );
    }
    
    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection); 
    }
    
    public void SetupSuccessSpeciesContractsMock(Guid speciesId, Guid breedId)
    {
        var speciesDto = new SpeciesDto
        {
            Id = speciesId,
            Name = "Dog"
        };
        
        var breedDto = new BreedDto
        {
            Id = breedId,
            Name = "Labrador",
        };
        
        _speciesContractMock.GetSpecies(Arg.Any<CancellationToken>())
            .Returns([speciesDto.Id]);
        
        _speciesContractMock.GetBreedsBySpeciesId(Arg.Any<Guid>(),Arg.Any<CancellationToken>())
            .Returns([breedDto.Id]);
    }
    
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}