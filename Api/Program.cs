using Api.Authentication;
using Application.Authentication;
using Application.Command;
using Application.Event;
using Application.IntegrationEvent;
using Application.Query;
using Application.Repository;
using Application.Service.MessageSender;
using Application.Storage;
using Application.UnitOfWork;
using Domain.Event;
using Infrastructure.Client.MongoDb;
using Infrastructure.Client.RabbitMq;
using Infrastructure.Command;
using Infrastructure.Event;
using Infrastructure.IntegrationEvent;
using Infrastructure.Query;
using Infrastructure.Repository;
using Infrastructure.Service.MessageSender;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;

namespace Api;

public static class Bootstrapper
{
    public static WebApplicationBuilder PrepareApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        AddPersistence(builder);
        AddDomainEvents(builder);
        AddIntegrationEvents(builder);
        AddCommands(builder);
        AddQueries(builder);
        AddApplicationServices(builder);
        AddAuthentication(builder);
        AddControllers(builder);

        return builder;
    }

    private static void AddPersistence(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
            builder.Services.AddSingleton<IAccountStorage, InMemoryAccountStorage>();
            builder.Services.AddSingleton<IEmailVerificationTokenStorage, InMemoryEmailVerificationTokenStorage>();
        }
        else
        {
            builder.Services.Configure<MongoDbClientOptions>(
                builder.Configuration.GetSection(MongoDbClientOptions.SectionName)
            );
            builder.Services.AddSingleton<MongoDbClient>();

            builder.Services.Configure<MongoDbAccountRepositoryOptions>(
                builder.Configuration.GetSection(MongoDbAccountRepositoryOptions.SectionName)
            );
            builder.Services.AddSingleton<IAccountRepository, MongoDbAccountRepository>();

            builder.Services.Configure<MongoDbAccountStorageOptions>(
                builder.Configuration.GetSection(MongoDbAccountStorageOptions.SectionName)
            );
            builder.Services.AddSingleton<IAccountStorage, MongoDbAccountStorage>();
            builder.Services.Configure<MongoDbEmailVerificationTokenStorageOptions>(
                builder.Configuration.GetSection(MongoDbEmailVerificationTokenStorageOptions.SectionName)
            );
            builder.Services.AddSingleton<IEmailVerificationTokenStorage, MongoDbEmailVerificationTokenStorage>();
        }
    }

    private static void AddDomainEvents(WebApplicationBuilder builder)
    {
        RegisterEventHandler<AccountRegisteredEvent, AccountRegisteredEventHandler>(builder.Services);
        RegisterEventHandler<EmailVerifiedEvent, EmailVerifiedEventHandler>(builder.Services);

        builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();
    }

    private static void AddIntegrationEvents(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IIntegrationEventPublisher, InMemoryIntegrationEventPublisher>();
        }
        else
        {
            builder.Services.Configure<RabbitMqClientOptions>(
                builder.Configuration.GetSection(RabbitMqClientOptions.SectionName)
            );
            builder.Services.AddSingleton<RabbitMqClient>();

            builder.Services.Configure<RabbitMqIntegrationEventPublisherOptions>(
                builder.Configuration.GetSection(RabbitMqIntegrationEventPublisherOptions.SectionName)
            );
            builder.Services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        }
    }

    private static void AddCommands(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        RegisterCommandHandler<RegisterAccountCommand, RegisterAccountHandler, RegisterAccountResponse>(builder.Services);
        RegisterCommandHandler<VerifyEmailCommand, VerifyEmailHandler, VerifyEmailResponse>(builder.Services);

        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
    }

    private static void AddQueries(WebApplicationBuilder builder)
    {
        RegisterQueryHandler<GetAccountDetailQuery, GetAccountDetailHandler, GetAccountDetailResponse>(builder.Services);

        builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();
    }

    private static void AddApplicationServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMessageSender, ConsoleMessageSender>();
    }

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

        if (builder.Environment.IsDevelopment())
        {
            var authentication = builder.Services.AddAuthentication(DevelopmentAuthenticationHandler.SchemeName);
            authentication.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(DevelopmentAuthenticationHandler.SchemeName, null);
        }
        else
        {
            var authentication = builder.Services.AddAuthentication(JwtAuthenticationHandler.SchemeName);
            builder.Services.Configure<JwtAuthenticationOptions>(
                builder.Configuration.GetSection(JwtAuthenticationOptions.SectionName)
            );
            authentication.AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(JwtAuthenticationHandler.SchemeName, null);
        }

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("HasNickname", p => p.RequireClaim("nickname"));
        });
    }

    private static void AddControllers(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
    }

    private static void RegisterCommandHandler<TCommand, THandler, TResponse>(IServiceCollection services)
        where TCommand : ICommand
        where TResponse : ICommandResponse
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(provider => provider.GetRequiredService<THandler>());
    }

    private static void RegisterQueryHandler<TQuery, THandler, TResponse>(IServiceCollection services)
        where TQuery : IQuery
        where TResponse : IQueryResponse
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(provider => provider.GetRequiredService<THandler>());
    }

    private static void RegisterEventHandler<TEvent, THandler>(IServiceCollection services)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IEventHandler<TEvent>>(provider => provider.GetRequiredService<THandler>());
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var app = CreateWebApplication(args);
        app.Run();
    }

    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = Bootstrapper.PrepareApplicationBuilder(args);
        return ConfigureApplication(builder);
    }

    private static WebApplication ConfigureApplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapOpenApi();
        app.MapControllers();

        return app;
    }
}
