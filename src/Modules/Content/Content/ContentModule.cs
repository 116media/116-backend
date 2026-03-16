using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Content.Infrastructure.Repositories;
using _116.Content.Infrastructure.Services;
using _116.Shared.Application.Extensions;
using _116.Shared.Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Content;

/// <summary>
/// Provides extension methods to register and configure the Content module's services and middleware.
/// </summary>
public static class ContentModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Content module.
    /// </summary>
    private static ModuleOptions<ContentDbContext> GetModuleOptions()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        bool enableSeeding = !environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

        return new ModuleOptions<ContentDbContext>
        {
            ModuleName = ContentConstants.ModuleName,
            SchemaName = ContentConstants.SchemaName,
            EnableMigrations = enableSeeding,
            EnableSeeding = enableSeeding,
        };
    }

    /// <summary>
    /// Adds the Content module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddContentModule(this IServiceCollection services)
    {
        services.AddModuleDatabase(GetModuleOptions());

        // Register Mapster configuration and IMapper (thread-safe, no global state)
        TypeAdapterConfig mappingConfig = MappingRegistration.CreateConfiguration();
        services.AddSingleton(mappingConfig);
        services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<TypeAdapterConfig>()));

        services.AddScoped<IContentUnitOfWork, ContentUnitOfWork>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IShortVideoRepository, ShortVideoRepository>();
        services.AddScoped<ILyricsRepository, LyricsRepository>();
        services.AddHttpClient<IYoutubeThumbnailService, YoutubeThumbnailService>();
        services.AddScheduledJob<AbandonedDraftCleanupJob>(cronExpression: "0 0 * * * ?");
        services.AddScoped<ContentTypeSeeder>();

        return services;
    }

    /// <summary>
    /// Configures the Content module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder" /> for chaining.</returns>
    public static IApplicationBuilder UseContentModule(this IApplicationBuilder app)
    {
        ModuleOptions<ContentDbContext> options = GetModuleOptions();
        app.UseModuleDatabase(options);

        if (!options.EnableSeeding)
        {
            return app;
        }

        using IServiceScope scope = app.ApplicationServices.CreateScope();
        scope.ServiceProvider.GetRequiredService<ContentTypeSeeder>().SeedAllAsync().GetAwaiter().GetResult();

        return app;
    }
}
