using System.Security.Authentication;
using AnimalAllies.Accounts.Application.AccountManagement.Consumers.ApprovedVolunteerRequestEvent;
using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.Shared.Errors;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramBotService.Contracts;

namespace AnimalAllies.Accounts.Application.AccountManagement.Consumers.SendUserDataForAuthorizationEvent;

public class SendUserDataForAuthorizationEventConsumer: 
    IConsumer<TelegramBotService.Contracts.SendUserDataForAuthorizationEvent>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SendUserDataForAuthorizationEventConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendUserDataForAuthorizationEventConsumer(
        UserManager<User> userManager, 
        ILogger<SendUserDataForAuthorizationEventConsumer> logger,
        IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<TelegramBotService.Contracts.SendUserDataForAuthorizationEvent> context)
    {
        var  message = context.Message;
        
        var user = await _userManager.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == message.Email, context.CancellationToken);

        if (user is null)
            throw new InvalidCredentialException();

        var passwordConfirmed = await _userManager.CheckPasswordAsync(user, message.Password);
        if (!passwordConfirmed)
            throw new InvalidCredentialException();

        var messageEvent = new SendAuthorizationResponseEvent(message.ChatId, user.Id);
        
        await _publishEndpoint.Publish(messageEvent,  context.CancellationToken);
        
        _logger.LogInformation("User {email} authorized by telegram", user.Email);
    }
}