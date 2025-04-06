using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;
using NotificationService.Domain.Models;
using NotificationService.Options;

namespace NotificationService.Infrastructure.DbContext;

public class ApplicationDbContext(IConfiguration configuration): Microsoft.EntityFrameworkCore.DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .UseLoggerFactory(CreateLoggerFactory)
            .EnableSensitiveDataLogging()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type.FullName?.Contains("Configurations") ?? false);
        
        modelBuilder.HasDefaultSchema("notifications");
    }
    
    private static readonly ILoggerFactory CreateLoggerFactory
        = LoggerFactory.Create(builder => { builder.AddConsole(); });
    
    public DbSet<UserNotificationSettings> UserNotificationSettings => Set<UserNotificationSettings>();
}