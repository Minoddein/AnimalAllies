using System.Transactions;
using AnimalAllies.Accounts.Contracts.Responses;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Accounts.Domain.DomainEvents;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using FileService.Communication;
using FileService.Contract.Requests;
using FileService.Contract.Responses;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Commands.AddAvatar;

public class AddAvatarHandler : ICommandHandler<AddAvatarCommand, AddAvatarResponse>
{
    private readonly FileHttpClient _fileHttpClient;
    private readonly ILogger<AddAvatarHandler> _logger;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<AddAvatarCommand> _validator;

    public AddAvatarHandler(
        ILogger<AddAvatarHandler> logger,
        IValidator<AddAvatarCommand> validator,
        [FromKeyedServices(Constraints.Context.Accounts)]
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        FileHttpClient fileHttpClient,
        IPublisher publisher)
    {
        _logger = logger;
        _validator = validator;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _fileHttpClient = fileHttpClient;
        _publisher = publisher;
    }

    public async Task<Result<AddAvatarResponse>> Handle(
        AddAvatarCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult = await _validator.ValidateAsync(command, cancellationToken)
            .ConfigureAwait(false);
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
            User? user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
                .ConfigureAwait(false);
            if (user is null)
            {
                return Errors.General.NotFound();
            }

            UploadPresignedUrlRequest request = new(
                command.UploadFileDto.BucketName,
                command.UploadFileDto.FileName,
                command.UploadFileDto.ContentType);

            GetUploadPresignedUrlResponse? response =
                await _fileHttpClient.GetUploadPresignedUrlAsync(request, cancellationToken);
            if (response is null)
            {
                return Errors.General.Null("response from file service is null");
            }

            user.Photo = response.FileId + response.Extension;

            AddAvatarResponse addAvatarResponse = new(response.UploadUrl);

            UserAddedAvatarDomainEvent @event = new(user.Id);

            await _publisher.Publish(@event, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            scope.Complete();

            _logger.LogInformation("Added avatar to user with id {id}", command.UserId);

            return addAvatarResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding avatar to user with id {id}", command.UserId);

            return Error.Failure("fail.to.add.avatar", "Fail to add avatar to user");
        }
    }
}