using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Enpoints;
using NotificationService.Infrastructure.DbContext;

namespace NotificationService.Features.GetUserNotificationSettings;

public class GetUserNotificationSettings
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/Notifications/user-notification-settings/{userId:guid}", Handler);
        }
    }
    
    private static async Task<IResult> Handler( 
        Guid userId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.UserNotificationSettings.FirstOrDefaultAsync(u => 
            u.UserId == userId, cancellationToken);
        
        return settings is null ? Results.NotFound() : Results.Ok(settings);
    }
}