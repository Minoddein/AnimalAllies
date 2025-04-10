﻿using System.Data;
using System.Text;
using System.Text.Json;
using AnimalAllies.Accounts.Domain;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Accounts.Application.AccountManagement.Queries.GetUserById;

public class GetUserByIdHandler: IQueryHandler<UserDto?, GetUserByIdQuery>
{
    private readonly ILogger<GetUserByIdHandler> _logger;
    private readonly IValidator<GetUserByIdQuery> _validator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUserByIdHandler(
        ILogger<GetUserByIdHandler> logger,
        IValidator<GetUserByIdQuery> validator, 
        [FromKeyedServices(Constraints.Context.Accounts)]ISqlConnectionFactory sqlConnectionFactory)
    {
        _logger = logger;
        _validator = validator;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<UserDto?>> Handle(
        GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();
        
        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();
        
        parameters.Add("@UserId", query.UserId);

        var user = (await GetUser(connection, parameters)).FirstOrDefault();
        
        return user;
    }
    

    private static async Task<IEnumerable<UserDto>> GetUser(IDbConnection connection, DynamicParameters parameters)
    {
        var sql = new StringBuilder("""
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

        /*var user = await connection
            .QueryAsync<UserDto, RoleDto,VolunteerAccountDto?,ParticipantAccountDto?,string,string, string, UserDto>(
                sql.ToString(),
                (user, role,volunteer,participant, jsonSocialNetworks, jsonRequisites, jsonCertificates) =>
                {
                    var socialNetworks = JsonSerializer
                        .Deserialize<SocialNetworkDto[]>(jsonSocialNetworks) ?? [];
                    user.SocialNetworks = socialNetworks;

                    if (volunteer is not null)
                    {
                        volunteer.Requisites = jsonRequisites != null
                            ? JsonSerializer.Deserialize<RequisiteDto[]>(jsonRequisites)
                            : [];
                        
                        volunteer.Certificates = jsonCertificates != null 
                            ? JsonSerializer.Deserialize<CertificateDto[]>(jsonCertificates)
                            : [];
                        
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
                splitOn: "role_id, volunteer_id, participant_id, social_networks, requisites, certificates"
            );*/
        
        var user = await connection
            .QueryAsync<UserDto, RoleDto, VolunteerAccountDto?, ParticipantAccountDto?, SocialNetworkDto[], RequisiteDto[], CertificateDto[], UserDto>(
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

                    user.Roles = new[] { role };
            
                    return user;
                },
                param: parameters,
                splitOn: "role_id, volunteer_id, participant_id, social_networks, requisites, certificates"
            );
        return user;
    }
}