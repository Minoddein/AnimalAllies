using AnimalAllies.Accounts.Infrastructure.Seeding;
using AnimalAllies.Framework.Middlewares;
using AnimalAllies.Web;
using AnimalAllies.Web.Extensions;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

var accountsSeeder = app.Services.GetRequiredService<AccountsSeeder>();

await accountsSeeder.SeedAsync();

app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() | app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //await app.ApplyMigrations();
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


app.Run();

public partial class Program;