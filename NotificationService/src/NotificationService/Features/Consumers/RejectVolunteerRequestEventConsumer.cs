using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Contracts.Requests;
using NotificationService.Domain.Models;
using NotificationService.Infrastructure.DbContext;
using NotificationService.Infrastructure.Services;
using NotificationService.Validators;

namespace NotificationService.Features.Consumers;

public class RejectVolunteerRequestEventConsumer: IConsumer<SendNotificationRejectVolunteerRequestEvent>
{
    private readonly MailSenderService _mailService;
    private readonly ILogger<RejectVolunteerRequestEventConsumer> _logger;
    private readonly EmailValidator _emailValidator;
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RejectVolunteerRequestEventConsumer(
        MailSenderService mailService,
        ILogger<RejectVolunteerRequestEventConsumer> logger,
        EmailValidator emailValidator,
        ApplicationDbContext context,
        IPublishEndpoint publishEndpoint)
    {
        _mailService = mailService;
        _logger = logger;
        _emailValidator = emailValidator;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SendNotificationRejectVolunteerRequestEvent> context)
    {
        var message = context.Message;

        var validationResult = _emailValidator.Execute([message.UserEmail]);
        if (validationResult.IsFailure)
            throw new Exception("Invalid email format");
        
        var settings = await _context.UserNotificationSettings.FirstOrDefaultAsync(
            u => u.UserId == message.UserId, context.CancellationToken);
        
        if(settings is null)
            return;

        var description = "К сожалению, мы вынуждены вам отказать в волонтёрстве, по причине:" + message.RejectMessage;

        if (settings.EmailNotifications)
        {
            var mailData = new MailData(
                [message.UserEmail],
                "Отказ на заявку волонтёра",
                description);

            await _mailService.Send(mailData);
        }

        if (settings.TelegramNotifications)
        {
            var messageEvent = new SendTelegramNotificationEvent(settings.UserId, description);
            
            await _publishEndpoint.Publish(messageEvent, context.CancellationToken);
        }

        _logger.LogInformation("Sent notifications with reject volunteer request notification to user {email}",
            message.UserEmail);
    }
}