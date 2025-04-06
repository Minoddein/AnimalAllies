using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain;
using NotificationService.Domain.Models;
using NotificationService.Options;

namespace NotificationService.Infrastructure.Configurations;

public class UserNotificationSettingsConfiguration: IEntityTypeConfiguration<UserNotificationSettings>
{
    public void Configure(EntityTypeBuilder<UserNotificationSettings> builder)
    {
        builder.ToTable("user_notification_settings");

        builder.HasKey(s => s.Id)
            .HasName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id");

        builder.Property(s => s.EmailNotifications)
            .HasColumnName("email_notifications");
        
        builder.Property(s => s.WebNotifications)
            .HasColumnName("web_notifications");
        
        builder.Property(s => s.TelegramNotifications)
            .HasColumnName("telegram_notifications");
    }
}