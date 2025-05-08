using System.Data;
using AnimalAllies.Core.Database;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace VolunteerRequests.Infrastructure;

public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    private readonly IConfiguration _configuration = configuration;

    public IDbConnection Create()
        => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
}