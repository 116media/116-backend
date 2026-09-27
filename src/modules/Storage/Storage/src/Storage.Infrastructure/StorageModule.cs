using _116.BuildingBlocks.Application.Configurations;
using _116.BuildingBlocks.Application.Services;
using _116.BuildingBlocks.Infrastructure;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Storage.Application.Shared.Errors;
using _116.Storage.Application.Shared.Errors.Facade;
using _116.Storage.Application.Shared.Errors.Messages;
using _116.Storage.Application.Shared.EventHandlers;
using _116.Storage.Application.Shared.Exceptions.Handlers;
using _116.Storage.Application.Shared.Mappers;
using _116.Storage.Application.Shared.Persistence;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Application.Shared.Services;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Domain.Constants;
using _116.Storage.Domain.Events;
using _116.Storage.Infrastructure.BackgroundJobs;
using _116.Storage.Infrastructure.Outbox;
using _116.Storage.Infrastructure.Persistence;
using _116.Storage.Infrastructure.Repositories;
using _116.Storage.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _116.Storage.Infrastructure;

/// <summary>
/// Provides extension methods to register and configure the Storage module's services and middleware.
/// </summary>
public static class StorageModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Storage module.
    /// Migrations run in every environment except Testing; the module owns no seeders.
    /// </summary>
    /// <param name="environment">The host environment the options are derived from.</param>
    /// <returns>The module options for the supplied environment.</returns>
    private static ModuleOptions<StorageDbContext> GetModuleOptions() =>
        new() { ModuleName = StorageConstants.ModuleName, SchemaName = StorageConstants.SchemaName };

    /// <summary>
    /// Adds the Storage module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates at startup.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddCoreModule(builder.Environment);
    /// </code>
    /// </example>
    public static IServiceCollection AddCoreModule(this IServiceCollection services, IHostEnvironment environment)
    {
        // Register the database with base module infrastructure
        services.AddHttpCurrentActor();
        services.AddModuleDatabase(GetModuleOptions());

        // Register error message classes (IStringLocalizer-backed)
        services.AddScoped<ValidationErrorMessage>();
        services.AddScoped<InternalServerErrorMessage>();

        services.AddSingleton<IExceptionStrategy, DomainRuleExceptionStrategy>();

        // Register error factory classes
        services.AddScoped<FileErrors>();
        services.AddScoped<StorageI18n>();

        // Register Unit of Work for transaction management
        services.AddScoped<IStorageUnitOfWork, StorageUnitOfWork>();
        services.AddScoped(typeof(IStorageRepository<>), typeof(StorageRepository<>));
        services.AddScoped<IProcessedDomainEventStore, StorageProcessedDomainEventStore>();

        // Replay delivers events raised inside a transaction, not just retries failed dispatches.
        services.AddScheduledJob<StorageOutboxReplayJob>(cronExpression: "0 */1 * * * ?");

        // Sweeps uploads no referencing write ever claimed; the two cannot share a transaction.

        // Register core repositories
        // Contribute Storage mappings to the shared cross-module Mapster config
        services.AddModuleMappings(new MappingRegistration());

        services.AddScoped<IFileRepository, FileRepository>();

        // Register core management services
        services.AddScoped<IUrlSafetyGuard, UrlSafetyGuard>();
        services
            .AddHttpClient<IFileService, FileService>(client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler { AllowAutoRedirect = false, ConnectTimeout = TimeSpan.FromSeconds(5) }
            );
        services.AddHttpClient(
            CloudStorageResilience.HttpClientName,
            client => client.Timeout = CloudStorageResilience.Timeout
        );

        services.AddSingleton<ICloudStorageClient>(sp => new CloudinaryStorageClient(
            sp.GetRequiredService<CloudinarySettings>(),
            CloudStorageResilience.CreatePipeline(),
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(CloudStorageResilience.HttpClientName),
            sp.GetRequiredService<ILogger<CloudinaryStorageClient>>()
        ));
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IImageColorService, ImageColorService>();
        services.AddScoped<IFileUploadService, FileUploadService>();

        // The cross-module storage contract; other modules see only this seam.
        services.AddScoped<IFileStorageService, FileStorageService>();

        // File lifecycle domain event handlers
        services.AddScoped<IDomainEventHandler<FileReplacedEvent>, FileAssetCleanupHandler>();
        services.AddScoped<IDomainEventHandler<FileSoftDeletedEvent>, FileAssetCleanupHandler>();

        // File cache eviction: a replaced or deleted file must stop resolving from cache.
        services.AddScoped<IDomainEventHandler<FileReplacedEvent>, FileCacheHandler>();
        services.AddScoped<IDomainEventHandler<FileSoftDeletedEvent>, FileCacheHandler>();

        return services;
    }
}
