using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.Extension;
using AnimalAllies.Core.Models;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUsersWithPagination;

public class GetUsersWithPaginationHandler : IQueryHandler<PagedList<UserDto>, GetUsersWithPaginationQuery>
{
    private readonly ILogger<GetUsersWithPaginationHandler> _logger;
    private readonly IValidator<GetUsersWithPaginationQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUsersWithPaginationHandler(
        ILogger<GetUsersWithPaginationHandler> logger,
        IValidator<GetUsersWithPaginationQuery> validator,
        [FromKeyedServices(Constraints.Context.Accounts)]
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _logger = logger;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<PagedList<UserDto>>> Handle(
        GetUsersWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        parameters.Add("@Page", query.Page);
        parameters.Add("@PageSize", query.PageSize);

        var sql = new StringBuilder("""
                                    select
                                        u.id ,
                                        u.user_name,
                                        u.photo,
                                        u.is_banned,
                                        u.email,
                                        r.id as role_id,
                                        r.name as name,
                                        va.id as volunteer_id,
                                        va.first_name,
                                        va.second_name,
                                        va.patronymic,
                                        pa.id as participant_id,
                                        pa.first_name,
                                        pa.second_name,
                                        pa.patronymic,
                                        ap.id as admin_id,
                                        ap.first_name as admin_first_name,
                                        ap.second_name as admin_second_name,
                                        ap.patronymic as admin_patronymic
                                    from accounts.users u
                                             join accounts.role_user ru on u.id = ru.users_id
                                             join accounts.roles r on ru.roles_id = r.id
                                             left join accounts.participant_accounts pa on u.participant_account_id = pa.id
                                             left join accounts.volunteer_accounts va on u.volunteer_account_id = va.id
                                             left join accounts.admin_profiles ap on u.id = ap.user_id
                                    where u.email_confirmed = true
                                    """);

        sql.ApplyPagination(query.Page, query.PageSize);

        var users = await connection
            .QueryAsync<UserDto, RoleDto, VolunteerAccountDto?, ParticipantAccountDto?, AdminProfileDto?, UserDto>(
                sql.ToString(),
                (user, role, volunteer, participant, admin) =>
                {
                    if (admin is not null)
                    {
                        user.AdminProfile = admin;
                        user.AdminProfileId = admin.AdminUserId;
                    }
                    
                    if (volunteer is not null)
                    {
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
                param: parameters,
                splitOn: "role_id, volunteer_id, participant_id, admin_id"
            );

        _logger.LogInformation("Successfully retrieved users");

        return new PagedList<UserDto>
        {
            Items = users.ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM accounts.users")
        };
    }
}