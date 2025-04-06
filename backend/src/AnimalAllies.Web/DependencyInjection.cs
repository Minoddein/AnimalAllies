using AnimalAllies.Accounts.Application;
using AnimalAllies.Accounts.Application.AccountManagement.Consumers;
using AnimalAllies.Accounts.Application.AccountManagement.Consumers.ApprovedVolunteerRequestEvent;
using AnimalAllies.Accounts.Application.AccountManagement.Consumers.SendUserDataForAuthorizationEvent;
using AnimalAllies.Accounts.Infrastructure;
using AnimalAllies.Accounts.Presentation;
using AnimalAllies.Core.Dapper;
using AnimalAllies.Core.DTOs.Accounts;
using AnimalAllies.Core.DTOs.ValueObjects;
using AnimalAllies.Framework.Models;
using AnimalAllies.Species.Application;
using AnimalAllies.Species.Infrastructure;
using AnimalAllies.Species.Presentation;
using AnimalAllies.Volunteer.Application;
using AnimalAllies.Volunteer.Infrastructure;
using AnimalAllies.Volunteer.Presentation;
using Dapper;
using Discussion.Application;
using Discussion.Infrastructure;
using Discussion.Presentation;
using FileService.Communication;
using MassTransit;
using VolunteerRequests.Application;
using VolunteerRequests.Infrastructure;

namespace AnimalAllies.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAccountsManagementModule(configuration)
            .AddPetsManagementModule(configuration)
            .AddBreedsManagementModule(configuration)
            .AddVolunteerRequestsManagementModule(configuration)
            .AddDiscussionManagementModule(configuration)
            .AddFramework()
            .AddMessageBus(configuration)
            .AddFileService(configuration)
            .AddSqlMappers();
        
        return services;
    }

    private static IServiceCollection AddFileService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFileHttpCommunication(configuration);

        return services;
    }

    private static void AddSqlMappers(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(typeof(SocialNetworkDto[]), new JsonTypeHandler<SocialNetworkDto[]>());
        SqlMapper.AddTypeHandler(typeof(RequisiteDto[]), 
            new JsonTypeHandler<RequisiteDto[]>());
        SqlMapper.AddTypeHandler(typeof(CertificateDto[]), new JsonTypeHandler<CertificateDto[]>());
        SqlMapper.AddTypeHandler(typeof(PetPhotoDto[]), new JsonTypeHandler<PetPhotoDto[]>());
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();


            configure.AddConsumer<ApprovedVolunteerRequestEventConsumer>();
            configure.AddConsumer<SendUserDataForAuthorizationEventConsumer>();
            
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"]!), h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.Durable = true;
                
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
    
    private static IServiceCollection AddFramework(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<UserScopedData>();

        return services;
    }

    private static IServiceCollection AddVolunteerRequestsManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddVolunteerRequestsInfrastructure(configuration)
            .AddVolunteerRequestsApplication();
        
        return services;
    }
    
    private static IServiceCollection AddDiscussionManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDiscussionInfrastructure(configuration)
            .AddDiscussionApplication()
            .AddDiscussionPresentation();
        
        return services;
    }
    
    private static IServiceCollection AddAccountsManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAccountsPresentation()
            .AddAccountsApplication()
            .AddAccountsInfrastructure(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddPetsManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddVolunteerPresentation()
            .AddVolunteerApplication()
            .AddVolunteerInfrastructure(configuration);

        return services;
    }
    
    private static IServiceCollection AddBreedsManagementModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSpeciesPresentation()
            .AddSpeciesApplication()
            .AddSpeciesInfrastructure(configuration);

        return services;
    }
}