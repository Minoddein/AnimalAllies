using System.Transactions;
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

public class ApprovedVolunteerRequestEventConsumer:
    IConsumer<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<ApprovedVolunteerRequestEventConsumer> _logger;
    private readonly IAccountManager _accountManager;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox;
    
    public ApprovedVolunteerRequestEventConsumer(
        UserManager<User> userManager,
        IAccountManager accountManager,
        ILogger<ApprovedVolunteerRequestEventConsumer> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWorkOutbox, 
        RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _accountManager = accountManager;
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWorkOutbox = unitOfWorkOutbox;
        _roleManager = roleManager;
    }

    public async Task Consume(ConsumeContext<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent> context)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var message = context.Message;

            var user = await _userManager.Users
                .Include(u => u.VolunteerAccount)
                .FirstOrDefaultAsync(u => u.Id == context.Message.UserId);

            if (user is null || user.VolunteerAccount is not null)
                throw new Exception(Errors.General.NotFound(context.Message.UserId).ErrorMessage);
            
            var fullName = FullName.Create(
                message.FirstName,
                message.SecondName,
                message.Patronymic).Value;

            var volunteer = new VolunteerAccount(fullName, message.WorkExperience, user)
            {
                Phone = PhoneNumber.Create(message.Phone).Value
            };
            
            
            user.VolunteerAccount = volunteer;
            user.VolunteerAccountId = volunteer.Id;

            await _accountManager.CreateVolunteerAccount(volunteer);
            
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r =>
                r.Name == "Volunteer", context.CancellationToken);

            if (role is null)
                throw new Exception(Errors.General.NotFound().ErrorMessage);
            
            user.AddRole(role);
            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);

            var key = TagsConstants.USERS + "_" + message.UserId;

            var integrationEvent = new CacheInvalidateIntegrationEvent(key, null);
            
            await _outboxRepository.AddAsync(integrationEvent, context.CancellationToken);

            await _unitOfWorkOutbox.SaveChanges(context.CancellationToken);
            
            transaction.Complete();

            _logger.LogInformation("created volunteer account to user with id {userId}", user.Id);
        }
        catch (Exception)
        {
            _logger.LogError("failed to create volunteer account");
        }
        
    }
}

public class ApprovedVolunteerRequestEventConsumerDefinition : ConsumerDefinition<ApprovedVolunteerRequestEventConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ApprovedVolunteerRequestEventConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(c =>
        {
            c.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        });
    }
}