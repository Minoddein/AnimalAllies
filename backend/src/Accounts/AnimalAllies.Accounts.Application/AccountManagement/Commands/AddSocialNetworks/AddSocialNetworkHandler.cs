using System.Transactions;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddSocialNetworks;

public class AddSocialNetworkHandler: ICommandHandler<AddSocialNetworkCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AddSocialNetworkHandler> _logger;
    private readonly IValidator<AddSocialNetworkCommand> _validator;
    private readonly IPublisher _publisher;

    public AddSocialNetworkHandler(
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork,
        UserManager<User> userManager, 
        ILogger<AddSocialNetworkHandler> logger, 
        IValidator<AddSocialNetworkCommand> validator,
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result> Handle(AddSocialNetworkCommand command, CancellationToken cancellationToken = default)
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
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            if (user is null)
                return Errors.General.NotFound();

            var socialNetworks = command.SocialNetworkDtos
                .Select(s => SocialNetwork.Create(s.Title, s.Url).Value);

            user.AddSocialNetwork(socialNetworks);

            var @event = new UserAddedSocialNetworkDomainEvent(user.Id);

            await _publisher.Publish(@event, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("Added social networks to user with id {id}", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding social networks to user with id {id}", command.UserId);

            return Error.Failure("fail.to.add.social_networks", 
                "Fail to add social networks to user");
        }
    }
}