using AnimalAllies.Accounts.Infrastructure.Seeding;
using AnimalAllies.Framework.Middlewares;
using AnimalAllies.Web;
using AnimalAllies.Web.Extensions;
using DotNetEnv;
using Serilog;

Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogger(builder.Configuration);

builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
});

builder.Services.AddSerilog();

builder.Services.AddModules(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    /*options.AddPolicy("NextApp", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });*/
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwagger();

WebApplication app = builder.Build();

AccountsSeeder accountsSeeder = app.Services.GetRequiredService<AccountsSeeder>();

await accountsSeeder.SeedAsync().ConfigureAwait(false);

app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() | (app.Environment.EnvironmentName == "Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // await app.ApplyMigrations();
}

app.UseCors(builder =>
{
    builder
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseScopeDataMiddleware();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program;