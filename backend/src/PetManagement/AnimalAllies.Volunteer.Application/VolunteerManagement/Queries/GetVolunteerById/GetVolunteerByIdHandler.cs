using System.Data;
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
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteerById;

public class GetVolunteerByIdHandler(
    ILogger<GetVolunteerByIdHandler> logger,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    ISqlConnectionFactory sqlConnectionFactory,
    IValidator<GetVolunteerByIdQuery> validator) : IQueryHandler<VolunteerDto, GetVolunteerByIdQuery>
{
    private readonly ILogger<GetVolunteerByIdHandler> _logger = logger;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly IValidator<GetVolunteerByIdQuery> _validator = validator;

    public async Task<Result<VolunteerDto>> Handle(
        GetVolunteerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validatorResult =
            await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToErrorList();
        }

        IDbConnection connection = _sqlConnectionFactory.Create();

        DynamicParameters parameters = new();

        parameters.Add("@VolunteerId", query.VolunteerId);

        StringBuilder sql = new("""
                                select 
                                id,
                                first_name,
                                second_name,
                                patronymic,
                                description,
                                email,
                                phone_number,
                                work_experience,
                                requisites
                                from volunteers.volunteers
                                where id = @VolunteerId
                                limit 1
                                """);

        IEnumerable<VolunteerDto> volunteerQuery =
            await connection.QueryAsync<VolunteerDto, RequisiteDto[], VolunteerDto>(
                sql.ToString(),
                (volunteer, requisites) =>
                {
                    volunteer.Requisites = requisites;

                    return volunteer;
                },
                splitOn: "requisites",
                param: parameters).ConfigureAwait(false);

        VolunteerDto? result = volunteerQuery.FirstOrDefault();

        if (result is null)
        {
            return Errors.General.NotFound();
        }

        _logger.LogInformation("Get volunteer with id {VolunteerId}", query.VolunteerId);

        return result;
    }
}