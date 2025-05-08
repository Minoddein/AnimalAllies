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
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddSocialNetworks;

public class AddSocialNetworkHandler(
    [FromKeyedServices(Constraints.Context.Accounts)]
    IUnitOfWork unitOfWork,
    UserManager<User> userManager,
    ILogger<AddSocialNetworkHandler> logger,
    IValidator<AddSocialNetworkCommand> validator,
    IPublisher publisher) : ICommandHandler<AddSocialNetworkCommand>
{
    private readonly ILogger<AddSocialNetworkHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IValidator<AddSocialNetworkCommand> _validator = validator;

    public async Task<Result> Handle(AddSocialNetworkCommand command, CancellationToken cancellationToken = default)
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
            User? user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
                .ConfigureAwait(false);
            if (user is null)
            {
                return Errors.General.NotFound();
            }

            IEnumerable<SocialNetwork> socialNetworks = command.SocialNetworkDtos
                .Select(s => SocialNetwork.Create(s.Title, s.Url).Value);

            user.AddSocialNetwork(socialNetworks);

            UserAddedSocialNetworkDomainEvent @event = new(user.Id);

            await _publisher.Publish(@event, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            scope.Complete();

            _logger.LogInformation("Added social networks to user with id {id}", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding social networks to user with id {id}", command.UserId);

            return Error.Failure(
                "fail.to.add.social_networks",
                "Fail to add social networks to user");
        }
    }
}