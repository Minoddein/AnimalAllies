using System.Transactions;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.UpdateCertificates;

public class UpdateCertificatesHandler: ICommandHandler<UpdateCertificatesCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UpdateCertificatesHandler> _logger;
    private readonly IValidator<UpdateCertificatesCommand> _validator;
    private readonly IPublisher _publisher;

    public UpdateCertificatesHandler(
        [FromKeyedServices(Constraints.Context.Accounts)]IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ILogger<UpdateCertificatesHandler> logger,
        IValidator<UpdateCertificatesCommand> validator, 
        IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _validator = validator;
        _publisher = publisher;
    }

    public async Task<Result> Handle(UpdateCertificatesCommand command, CancellationToken cancellationToken = default)
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
            var user = await _userManager.Users
                .Include(u => u.VolunteerAccount)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            
            if (user?.VolunteerAccount is null)
                return Errors.General.NotFound();

            var certificates = command.Certificates
                .Select(c => Certificate.Create(
                    c.Title,
                    c.IssuingOrganization,
                    c.IssueDate,
                    c.ExpirationDate,
                    c.Description).Value);

            user.VolunteerAccount.UpdateCertificates(certificates);

            //var @event = new UserAddedSocialNetworkDomainEvent(user.Id);

            //await _publisher.Publish(@event, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("Added certificates to user with id {id}", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding certificates to user with id {id}", command.UserId);

            return Error.Failure("fail.to.add.certificates", 
                "Fail to add certificates to user");
        }
    }
}