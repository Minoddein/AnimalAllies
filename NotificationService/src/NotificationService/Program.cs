using Hangfire;
using NotificationService;
using NotificationService.Api.Extensions;
using NotificationService.Api.Middlewares;


var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApp(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpoints();

var app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}

app.UseCors("CorsPolicy");

app.UseHangfireServer();

app.MapEndpoints();

app.Run();