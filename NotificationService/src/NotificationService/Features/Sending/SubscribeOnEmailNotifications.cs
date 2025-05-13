using Hangfire;
using NotificationService.Api.Enpoints;
using NotificationService.Infrastructure.Services;
using NotificationService.Jobs;
using NotificationService.Validators;

namespace NotificationService.Features.Sending;

/// <summary>
/// Подписаться на события отправки уведомлений
/// </summary>
public class SubscribeOnEmailNotifications
{
    private record SubscribeOnEmailNotificationsRequest(
        string Reciever, 
        DateTime CompetitionDate,
        string Subject,
        string Body);
    
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Notifications/email-notification", Handler);
        }
    }
    
    /// <summary>
    /// Обработчик отправления уведомлений с фильтрацией
    /// </summary>
    /// <param name="request">Принимаемый запрос</param>
    /// <param name="service">Сервис отправки почтовых сообщений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    private static async Task<IResult> Handler( 
        SubscribeOnEmailNotificationsRequest request,
        MailSenderService service,
        EmailValidator validator,
        CancellationToken cancellationToken = default)
    {
        List<string> recievers = [request.Reciever];

        var validationResult = validator.Execute(recievers);
        if (validationResult.IsFailure)
            return Results.BadRequest(validationResult.Error);
        
        //TODO: Для теста в минутах: через 1,2,3
        
        var oneMonthBefore = request.CompetitionDate.AddMinutes(1);
        BackgroundJob.Schedule<SendEmailJob>(job => 
            job.Execute(recievers, request.Subject, request.Body), oneMonthBefore);
        
        var oneWeekBefore = request.CompetitionDate.AddMinutes(2);
        BackgroundJob.Schedule<SendEmailJob>(job => 
            job.Execute(recievers, request.Subject, request.Body), oneWeekBefore);
        
        var twoDaysBefore = request.CompetitionDate.AddMinutes(3);
        BackgroundJob.Schedule<SendEmailJob>(job => 
            job.Execute(recievers, request.Subject, request.Body), twoDaysBefore);
        
        return Results.Ok();
    }
}