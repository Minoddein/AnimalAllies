using System.Text;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteerById;

public class GetVolunteerByIdHandler : IQueryHandler<VolunteerDto, GetVolunteerByIdQuery>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ILogger<GetVolunteerByIdHandler> _logger;
    private readonly IValidator<GetVolunteerByIdQuery> _validator;

    public GetVolunteerByIdHandler(
        ILogger<GetVolunteerByIdHandler> logger,
        [FromKeyedServices(Constraints.Context.PetManagement)]
        ISqlConnectionFactory sqlConnectionFactory,
        IValidator<GetVolunteerByIdQuery> validator)
    {
        _logger = logger;
        _sqlConnectionFactory = sqlConnectionFactory;
        _validator = validator;
    }


    public async Task<Result<VolunteerDto>> Handle(
        GetVolunteerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validatorResult.IsValid)
            return validatorResult.ToErrorList();

        var connection = _sqlConnectionFactory.Create();

        var parameters = new DynamicParameters();

        parameters.Add("@VolunteerId", query.VolunteerId);

        var sql = new StringBuilder("""
                                    select 
                                    v.id,
                                    relation_id,
                                    u.photo as avatar_url,
                                    first_name,
                                    second_name,
                                    patronymic,
                                    description,
                                    v.email,
                                    v.phone_number,
                                    work_experience,
                                    requisites
                                    from volunteers.volunteers v
                                    inner join accounts.users u on u.id = v.relation_id
                                    where v.id = @VolunteerId or relation_id = @VolunteerId
                                    limit 1
                                    """);

        var volunteerQuery = await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
            sql.ToString(),
            (volunteer, requisites) =>
            {
                volunteer.Requisites = requisites;

                return volunteer;
            },
            splitOn: "requisites",
            param: parameters);

        var result = volunteerQuery.FirstOrDefault();

        if (result is null)
            return Errors.General.NotFound();

        _logger.LogInformation("Get volunteer with id {VolunteerId}", query.VolunteerId);

        return result;
    }
}