using Microsoft.EntityFrameworkCore;
using TelegramBotService.Models;

namespace TelegramBotService.Infrastructure;

public class ApplicationDbContext(IConfiguration configuration): DbContext
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
        modelBuilder.HasDefaultSchema("telegram");
        
        modelBuilder.Entity<TelegramUser>().ToTable("telegram_users");
        modelBuilder.Entity<TelegramUser>().HasKey(x => x.Id)
            .HasName("id");
        
        modelBuilder.Entity<TelegramUser>().Property(x => x.UserId)
            .HasColumnName("user_id");
        
        modelBuilder.Entity<TelegramUser>().Property(x => x.ChatId)
            .HasColumnName("chat_id");
    }

    private static readonly ILoggerFactory CreateLoggerFactory
        = LoggerFactory.Create(builder => { builder.AddConsole(); });

    public DbSet<TelegramUser> TelegramUsers { get; set; } = null!;
}