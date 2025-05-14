using System.Transactions;
using AnimalAllies.Accounts.Application.Managers;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAccountManager _accountManager;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox;
    
    public RegisterUserHandler(
        UserManager<User> userManager,
        ILogger<RegisterUserHandler> logger,
        IValidator<RegisterUserCommand> validator,
        RoleManager<Role> roleManager, 
        IAccountManager accountManager,
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWorkOutbox)
    {
        _userManager = userManager;
        _logger = logger;
        _validator = validator;
        _roleManager = roleManager;
        _accountManager = accountManager;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
        _unitOfWorkOutbox = unitOfWorkOutbox;
    }
    
    public async Task<Result> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == ParticipantAccount.Participant, cancellationToken);
            if (role is null)
                return Errors.General.NotFound();

            var isExistWithSameName =
                await _userManager.Users.FirstOrDefaultAsync(u => u.UserName!.Equals(command.UserName),
                    cancellationToken);

            if (isExistWithSameName is not null)
                return Errors.General.AlreadyExist();

            var user = User.CreateParticipant(command.UserName, command.Email, role);

            var result = await _userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
                Error.Failure("cannot.create.user","Can not create user");
            
            var resultAddToRole = await _userManager.AddToRoleAsync(user, "Participant");
            if(!resultAddToRole.Succeeded)
                Error.Failure("cannot.create.user","Can not create user");
            
            var fullName = FullName.Create(
                command.FullNameDto.FirstName,
                command.FullNameDto.SecondName,
                command.FullNameDto.Patronymic).Value;

            var participantAccount = new ParticipantAccount(fullName, user);
            
            await _accountManager.CreateParticipantAccount(participantAccount, cancellationToken);

            user.ParticipantAccount = participantAccount;
            user.ParticipantAccountId = participantAccount.Id;

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var message = new SendConfirmTokenByEmailEvent(user.Id, user.Email!, code);

            await _outboxRepository.AddAsync(message, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);
            await _unitOfWorkOutbox.SaveChanges(cancellationToken);
            
            scope.Complete();
                
            _logger.LogInformation("User created:{name} a new account with password", command.UserName);

            return Result.Success();
        }
        catch(Exception ex)
        {
            _logger.LogError("Registration of user fall with error");
            
            return Error.Failure("cannot.create.user","Can not create user");
        }
    }
}