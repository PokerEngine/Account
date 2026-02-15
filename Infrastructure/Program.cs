using Application.Command;
using Application.Event;
using Application.IntegrationEvent;
using Application.Query;
using Application.Repository;
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
using Infrastructure.Storage;

namespace Infrastructure;

public static class Bootstrapper
{
    public static WebApplicationBuilder PrepareApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddOpenApi();

        // Register clients
        builder.Services.Configure<MongoDbClientOptions>(
            builder.Configuration.GetSection(MongoDbClientOptions.SectionName)
        );
        builder.Services.AddSingleton<MongoDbClient>();
        builder.Services.Configure<RabbitMqClientOptions>(
            builder.Configuration.GetSection(RabbitMqClientOptions.SectionName)
        );
        builder.Services.AddSingleton<RabbitMqClient>();

        // Register repository
        builder.Services.Configure<MongoDbRepositoryOptions>(
            builder.Configuration.GetSection(MongoDbRepositoryOptions.SectionName)
        );
        builder.Services.AddSingleton<IRepository, MongoDbRepository>();

        // Register storage
        builder.Services.Configure<MongoDbStorageOptions>(
            builder.Configuration.GetSection(MongoDbStorageOptions.SectionName)
        );
        builder.Services.AddSingleton<IStorage, MongoDbStorage>();

        // Register unit of work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register commands
        RegisterCommandHandler<RegisterAccountCommand, RegisterAccountHandler, RegisterAccountResponse>(builder.Services);
        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Register queries
        RegisterQueryHandler<GetAccountDetailQuery, GetAccountDetailHandler, GetAccountDetailResponse>(builder.Services);
        builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Register domain events
        RegisterEventHandler<AccountRegisteredEvent, AccountRegisteredEventHandler>(builder.Services);
        builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Register integration events
        builder.Services.Configure<RabbitMqIntegrationEventPublisherOptions>(
            builder.Configuration.GetSection(RabbitMqIntegrationEventPublisherOptions.SectionName)
        );
        builder.Services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        return builder;
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

    // Public method for creating the WebApplication - can be called by tests
    // This allows WebApplicationFactory to work properly with the minimal hosting model
    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = Bootstrapper.PrepareApplicationBuilder(args);
        return ConfigureApplication(builder);
    }

    // Configure the application pipeline
    private static WebApplication ConfigureApplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.MapOpenApi();
        app.MapControllers();

        return app;
    }
}
