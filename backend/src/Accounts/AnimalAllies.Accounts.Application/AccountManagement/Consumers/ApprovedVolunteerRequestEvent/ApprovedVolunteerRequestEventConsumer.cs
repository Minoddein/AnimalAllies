using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Consumers.ApprovedVolunteerRequestEvent;

public class ApprovedVolunteerRequestEventConsumer(
    UserManager<User> userManager,
    IAccountManager accountManager,
    ILogger<ApprovedVolunteerRequestEventConsumer> logger,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWorkOutbox) :
    IConsumer<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent>
{
    private readonly IAccountManager _accountManager = accountManager;
    private readonly ILogger<ApprovedVolunteerRequestEventConsumer> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox = unitOfWorkOutbox;
    private readonly UserManager<User> _userManager = userManager;

    public async Task Consume(
        ConsumeContext<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent> context)
    {
        VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent message = context.Message;

        User? user =
            await _userManager.Users.FirstOrDefaultAsync(u => u.Id == context.Message.UserId).ConfigureAwait(false) ??
            throw new Exception(Errors.General.NotFound(context.Message.UserId).ErrorMessage);

        FullName fullName = FullName.Create(
            message.FirstName,
            message.SecondName,
            message.Patronymic).Value;

        VolunteerAccount volunteer = new(fullName, message.WorkExperience, user);
        user.VolunteerAccount = volunteer;
        user.VolunteerAccountId = volunteer.Id;

        await _accountManager.CreateVolunteerAccount(volunteer).ConfigureAwait(false);

        string key = TagsConstants.USERS + "_" + message.UserId;

        CacheInvalidateIntegrationEvent integrationEvent = new(key, null);

        await _outboxRepository.AddAsync(integrationEvent, context.CancellationToken).ConfigureAwait(false);

        await _unitOfWorkOutbox.SaveChanges(context.CancellationToken).ConfigureAwait(false);

        _logger.LogInformation("created volunteer account to user with id {userId}", user.Id);
    }
}

public class ApprovedVolunteerRequestEventConsumerDefinition : ConsumerDefinition<ApprovedVolunteerRequestEventConsumer>
{
    [Obsolete]
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ApprovedVolunteerRequestEventConsumer> consumerConfigurator) =>
        endpointConfigurator.UseMessageRetry(c =>
        {
            c.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        });
}