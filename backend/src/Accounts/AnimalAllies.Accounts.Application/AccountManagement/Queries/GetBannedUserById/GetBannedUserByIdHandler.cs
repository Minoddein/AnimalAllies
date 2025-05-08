using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetBannedUserById;

public class GetBannedUserByIdHandler(
    [FromKeyedServices(Constraints.Context.Accounts)]
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<GetBannedUserByIdHandler> logger,
    IValidator<GetBannedUserByIdQuery> validator) : IQueryHandler<ProhibitionSendingDto, GetBannedUserByIdQuery>
{
    private readonly ILogger<GetBannedUserByIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetBannedUserByIdQuery> _validator = validator;

    public async Task<Result<ProhibitionSendingDto>> Handle(
        GetBannedUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@UserId", query.UserId);

        StringBuilder sql = new("""
                                select
                                    u.id,
                                    u.user_id,
                                    u.banned_at
                                from accounts.banned_users u
                                where u.user_id = @UserId
                                """);

        List<ProhibitionSendingDto> bannedUser =
        [
            .. await connection
                .QueryAsync<ProhibitionSendingDto>(sql.ToString(), parameters).ConfigureAwait(false)
        ];

        ProhibitionSendingDto? result = bannedUser.SingleOrDefault();

        if (result is null)
        {
            return Errors.General.NotFound();
        }

        _logger.LogInformation("got user with id {id}", query.UserId);

        return result;
    }

    public async Task<Result<ProhibitionSendingDto>>
        Handle(Guid userId, CancellationToken cancellationToken = default) =>
        await Handle(new GetBannedUserByIdQuery(userId), cancellationToken).ConfigureAwait(false);
}