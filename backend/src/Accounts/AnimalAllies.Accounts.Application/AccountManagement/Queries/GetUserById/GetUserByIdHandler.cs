using System.Data;
using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.CachingConstants;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUserById;

public class GetUserByIdHandler(
    ILogger<GetUserByIdHandler> logger,
    IValidator<GetUserByIdQuery> validator,
    [FromKeyedServices(Constraints.Context.Accounts)]
    ISqlConnectionFactory sqlConnectionFactory,
    HybridCache hybridCache) : IQueryHandler<UserDto?, GetUserByIdQuery>
{
    private readonly HybridCache _hybridCache = hybridCache;
    private readonly ILogger<GetUserByIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetUserByIdQuery> _validator = validator;

    public async Task<Result<UserDto?>> Handle(
        GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        HybridCacheEntryOptions options = new()
        {
            Expiration = TimeSpan.FromMinutes(8), LocalCacheExpiration = TimeSpan.FromMinutes(3)
        };

        UserDto? cacheUser = await _hybridCache.GetOrCreateAsync(
            TagsConstants.USERS + "_" + query.UserId,
            async _ =>
            {
                IDbConnection connection = _sqlConnectionFactory.Create();

                DynamicParameters parameters = new();

                parameters.Add("@UserId", query.UserId);

                return (await GetUser(connection, parameters).ConfigureAwait(false)).FirstOrDefault();
            },
            options,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Got user by id {userId}", cacheUser?.Id);

        return cacheUser;
    }

    private static async Task<IEnumerable<UserDto>> GetUser(IDbConnection connection, DynamicParameters parameters)
    {
        StringBuilder sql = new("""
                                select
                                    u.id ,
                                    u.user_name,
                                    u.photo,
                                    r.id as role_id,
                                    r.name as name,
                                    va.id as volunteer_id,
                                    va.first_name,
                                    va.second_name,
                                    va.patronymic,
                                    va.experience,
                                    pa.id as participant_id,
                                    pa.first_name,
                                    pa.second_name,
                                    pa.patronymic,
                                    u.social_networks as social_networks,
                                    va.requisites,
                                    va.certificates
                                from accounts.users u
                                         join accounts.role_user ru on u.id = ru.users_id
                                         join accounts.roles r on ru.roles_id = r.id
                                         left join accounts.participant_accounts pa on u.participant_account_id = pa.id
                                         left join accounts.volunteer_accounts va on u.volunteer_account_id = va.id
                                where u.id = @UserId limit 1
                                """);

        IEnumerable<UserDto> user = await connection
            .QueryAsync<UserDto, RoleDto, VolunteerAccountDto?, ParticipantAccountDto?, SocialNetworkDto[], RequisiteDto
                [], CertificateDto[], UserDto>(
                sql.ToString(),
                (user, role, volunteer, participant, socialNetworks, requisites, certificates) =>
                {
                    user.SocialNetworks = socialNetworks;

                    if (volunteer is not null)
                    {
                        volunteer.Requisites = requisites;
                        volunteer.Certificates = certificates;

                        user.VolunteerAccount = volunteer;
                        user.VolunteerAccountId = volunteer.VolunteerId;
                    }

                    if (participant is not null)
                    {
                        user.ParticipantAccount = participant;
                        user.ParticipantAccountId = participant.ParticipantId;
                    }

                    user.Roles = [role];

                    return user;
                },
                parameters,
                splitOn: "role_id, volunteer_id, participant_id, social_networks, requisites, certificates")
            .ConfigureAwait(false);
        return user;
    }
}