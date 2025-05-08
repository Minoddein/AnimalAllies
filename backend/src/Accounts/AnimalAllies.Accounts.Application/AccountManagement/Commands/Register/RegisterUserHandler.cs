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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Requests;
using Outbox.Abstractions;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.Register;

public class RegisterUserHandler(
    UserManager<User> userManager,
    ILogger<RegisterUserHandler> logger,
    IValidator<RegisterUserCommand> validator,
    RoleManager<Role> roleManager,
    IAccountManager accountManager,
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork,
    IOutboxRepository outboxRepository,
    IUnitOfWorkOutbox unitOfWorkOutbox) : ICommandHandler<RegisterUserCommand>
{
    private readonly IAccountManager _accountManager = accountManager;
    private readonly ILogger<RegisterUserHandler> _logger = logger;
    private readonly IOutboxRepository _outboxRepository = outboxRepository;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox = unitOfWorkOutbox;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IValidator<RegisterUserCommand> _validator = validator;

    public async Task<Result> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        using TransactionScope scope = new(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled
        );

        try
        {
            Role? role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == ParticipantAccount.Participant, cancellationToken)
                .ConfigureAwait(false);
            if (role is null)
            {
                return Errors.General.NotFound();
            }

            User? isExistWithSameName =
                await _userManager.Users.FirstOrDefaultAsync(
                    u => u.UserName!.Equals(command.UserName),
                    cancellationToken).ConfigureAwait(false);

            if (isExistWithSameName is not null)
            {
                return Errors.General.AlreadyExist();
            }

            User user = User.CreateParticipant(command.UserName, command.Email, role);

            IdentityResult result = await _userManager.CreateAsync(user, command.Password).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                Error.Failure("cannot.create.user", "Can not create user");
            }

            FullName fullName = FullName.Create(
                command.FullNameDto.FirstName,
                command.FullNameDto.SecondName,
                command.FullNameDto.Patronymic).Value;

            ParticipantAccount participantAccount = new(fullName, user);

            await _accountManager.CreateParticipantAccount(participantAccount, cancellationToken).ConfigureAwait(false);

            user.ParticipantAccount = participantAccount;
            user.ParticipantAccountId = participantAccount.Id;

            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);

            SendConfirmTokenByEmailEvent message = new(user.Id, user.Email!, code);

            await _outboxRepository.AddAsync(message, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);
            await _unitOfWorkOutbox.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("User created:{name} a new account with password", command.UserName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Registration of user fall with error");

            return Error.Failure("cannot.create.user", "Can not create user");
        }
    }
}