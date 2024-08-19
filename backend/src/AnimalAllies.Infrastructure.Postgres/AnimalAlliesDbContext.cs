using AnimalAllies.Domain.Models;
using AnimalAllies.Domain.Models.Pet;
using AnimalAllies.Domain.Models.Volunteer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Infrastructure;

public class AnimalAlliesDbContext: DbContext
{
    private readonly IConfiguration _configuration;
    
    public AnimalAlliesDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_configuration.GetConnectionString("DefaultConnection"))
            .UseLoggerFactory(CreateLoggerFactory)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnimalAlliesDbContext).Assembly);
    }

    private static readonly ILoggerFactory CreateLoggerFactory
        = LoggerFactory.Create(builder => { builder.AddConsole(); });

    public DbSet<Volunteer> Volunteers { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Species> Species { get; set; } = null!;
}