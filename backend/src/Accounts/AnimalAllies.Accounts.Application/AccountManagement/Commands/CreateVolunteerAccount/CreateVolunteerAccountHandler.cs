using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Contracts.Events;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.CreateVolunteerAccount;

public class CreateVolunteerAccountHandler: ICommandHandler<CreateVolunteerAccountCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<CreateVolunteerAccountHandler> _logger;
    private readonly IAccountManager _accountManager;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox;
    
    public CreateVolunteerAccountHandler(
        UserManager<User> userManager,
        IAccountManager accountManager,
        ILogger<CreateVolunteerAccountHandler> logger,
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

    public async Task<Result> Handle(
        CreateVolunteerAccountCommand command, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.VolunteerAccount)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user is null || user.VolunteerAccount is not null)
                throw new Exception(Errors.General.NotFound(command.UserId).ErrorMessage);
            
            var existingVolunteer = await _accountManager.GetVolunteerAccount(user.Id, cancellationToken);
            if (existingVolunteer is not null)
            {
                _logger.LogInformation("Volunteer account already exists for userId {UserId}, skipping", user.Id);
                return Errors.General.AlreadyExist();
            }
            
            var fullName = FullName.Create(
                command.FirstName,
                command.SecondName,
                command.Patronymic).Value;

            var volunteer = new VolunteerAccount(fullName, command.WorkExperience, user)
            {
                Phone = PhoneNumber.Create(command.Phone).Value
            };
            
            user.VolunteerAccount = volunteer;
            user.VolunteerAccountId = volunteer.Id;
            
            await _accountManager.CreateVolunteerAccount(volunteer, cancellationToken);
            
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r =>
                r.Name == "Volunteer", cancellationToken);

            if (role is null)
                throw new Exception(Errors.General.NotFound().ErrorMessage);
            
            user.AddRole(role);
            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);

            var key = TagsConstants.USERS + "_" + command.UserId;

            var integrationEvent = new CacheInvalidateIntegrationEvent(key, null);
            
            await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

            await _unitOfWorkOutbox.SaveChanges(cancellationToken);

            _logger.LogInformation("created volunteer account to user with id {userId}", user.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create volunteer account for userId {UserId}", command.UserId);
            
            return Error.Failure("fail.to.create.volunteer.account","failed to create volunteer account");
        }
    }
}