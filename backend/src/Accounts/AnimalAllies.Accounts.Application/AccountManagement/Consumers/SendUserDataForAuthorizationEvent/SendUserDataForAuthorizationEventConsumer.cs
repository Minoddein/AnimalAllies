using System.Security.Authentication;
using AnimalAllies.Accounts.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramBotService.Contracts;

namespace AnimalAllies.Accounts.Application.AccountManagement.Consumers.SendUserDataForAuthorizationEvent;

public class SendUserDataForAuthorizationEventConsumer(
    UserManager<User> userManager,
    ILogger<SendUserDataForAuthorizationEventConsumer> logger,
    IPublishEndpoint publishEndpoint) :
    IConsumer<TelegramBotService.Contracts.SendUserDataForAuthorizationEvent>
{
    private readonly ILogger<SendUserDataForAuthorizationEventConsumer> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly UserManager<User> _userManager = userManager;

    public async Task Consume(ConsumeContext<TelegramBotService.Contracts.SendUserDataForAuthorizationEvent> context)
    {
        TelegramBotService.Contracts.SendUserDataForAuthorizationEvent message = context.Message;

        User? user = await _userManager.Users
                         .Include(u => u.Roles)
                         .FirstOrDefaultAsync(u => u.Email == message.Email, context.CancellationToken)
                         .ConfigureAwait(false) ??
                     throw new InvalidCredentialException();

        bool passwordConfirmed = await _userManager.CheckPasswordAsync(user, message.Password).ConfigureAwait(false);
        if (!passwordConfirmed)
        {
            throw new InvalidCredentialException();
        }

        SendAuthorizationResponseEvent messageEvent = new(message.ChatId, user.Id);

        await _publishEndpoint.Publish(messageEvent, context.CancellationToken).ConfigureAwait(false);

        _logger.LogInformation("User {email} authorized by telegram", user.Email);
    }
}

public class SendUserDataForAuthorizationEventConsumerDefinition :
    ConsumerDefinition<SendUserDataForAuthorizationEventConsumer>
{
    [Obsolete]
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<SendUserDataForAuthorizationEventConsumer> consumerConfigurator) =>
        endpointConfigurator.UseMessageRetry(c =>
        {
            c.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        });
}