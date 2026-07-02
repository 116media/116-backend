using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Cache;
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

        // Register error message classes (IStringLocalizer-backed)
        services.AddScoped<ArticleErrorMessage>();
        services.AddScoped<VideoErrorMessage>();
        services.AddScoped<ShortVideoErrorMessage>();
        services.AddScoped<LyricsErrorMessage>();
        services.AddScoped<CategoryErrorMessage>();
        services.AddScoped<TagErrorMessage>();
        services.AddScoped<ContentTypeErrorMessage>();
        services.AddScoped<PricingTierErrorMessage>();
        services.AddScoped<PackageErrorMessage>();
        services.AddScoped<CustomerErrorMessage>();
        services.AddScoped<ContentOrderErrorMessage>();
        services.AddScoped<PlaylistErrorMessage>();
        services.AddScoped<ArticleInteractionErrorMessage>();
        services.AddScoped<ShortVideoInteractionErrorMessage>();
        services.AddScoped<PromotionLevelErrorMessage>();

        // Register error factory classes
        services.AddScoped<ArticleErrors>();
        services.AddScoped<VideoErrors>();
        services.AddScoped<ShortVideoErrors>();
        services.AddScoped<LyricsErrors>();
        services.AddScoped<CategoryErrors>();
        services.AddScoped<TagErrors>();
        services.AddScoped<ContentTypeErrors>();
        services.AddScoped<PricingTierErrors>();
        services.AddScoped<PackageErrors>();
        services.AddScoped<CustomerErrors>();
        services.AddScoped<ContentOrderErrors>();
        services.AddScoped<PlaylistErrors>();
        services.AddScoped<ArticleInteractionErrors>();
        services.AddScoped<ShortVideoInteractionErrors>();
        services.AddScoped<PromotionLevelErrors>();
        services.AddScoped<ContentI18n>();

        // Register Mapster configuration and IMapper (thread-safe, no global state)
        TypeAdapterConfig mappingConfig = MappingRegistration.CreateConfiguration();
        services.AddSingleton(mappingConfig);
        services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<TypeAdapterConfig>()));

        services.AddSingleton<IPopularTagsCacheInvalidator, PopularTagsCacheInvalidator>();
        services.AddSingleton<IPopularArticlesCacheInvalidator, PopularArticlesCacheInvalidator>();
        services.AddSingleton<IPopularVideosCacheInvalidator, PopularVideosCacheInvalidator>();

        services.AddScoped<IContentUnitOfWork, ContentUnitOfWork>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IShortVideoRepository, ShortVideoRepository>();
        services.AddScoped<ILyricsRepository, LyricsRepository>();
        services.AddScoped<IContentOrderRepository, ContentOrderRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();

        // Commerce factories
        services.AddScoped<IOrderPaymentFactory, OrderPaymentFactory>();
        services.AddScoped<IVerifyPaymentFactory, AdminVerifyPaymentFactory>();
        services.AddScoped<ISubmitOrderFactory, AdminSubmitOrderFactory>();
        services.AddScoped<IAddOrderItemFactory, AdminAddOrderItemFactory>();
        services.AddScoped<IAddItemTierFactory, AdminAddItemTierFactory>();
        services.AddScoped<ICreateOrderFactory, AdminCreateOrderFactory>();

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

        if (options.EnableSeeding)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            scope.ServiceProvider.GetRequiredService<ContentTypeSeeder>().SeedAllAsync().GetAwaiter().GetResult();
        }

        return app;
    }
}
