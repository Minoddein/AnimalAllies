using System.Collections.Concurrent;
using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IUnitOfWork _unitOfWork;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _userLocks = new();


    public ApprovedVolunteerRequestEventConsumer(
        UserManager<User> userManager, 
        RoleManager<Role> roleManager, 
        ILogger<ApprovedVolunteerRequestEventConsumer> logger, 
        IAccountManager accountManager, 
        IOutboxRepository outboxRepository, 
        IUnitOfWorkOutbox unitOfWorkOutbox,
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _accountManager = accountManager;
        _outboxRepository = outboxRepository;
        _unitOfWorkOutbox = unitOfWorkOutbox;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent> context)
    {
        /*using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);*/
        
        var userLock = _userLocks.GetOrAdd(context.Message.UserId, _ =>
            new SemaphoreSlim(1, 1));
        
        await userLock.WaitAsync();
        
        var transaction = await _unitOfWork.BeginTransaction();
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

            transaction.Commit();

            _logger.LogInformation("created volunteer account to user with id {userId}", user.Id);
        }
        catch (Exception)
        {
            transaction.Rollback();
            _logger.LogError("failed to create volunteer account");
        }
        finally
        {
            userLock.Release();
        }
    }
}