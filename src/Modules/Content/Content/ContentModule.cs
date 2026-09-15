using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Commerce.Services;
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
using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Exceptions.Handlers;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Content.Infrastructure.Repositories;
using _116.Content.Infrastructure.Services;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Extensions;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure;
using _116.Shared.Infrastructure.Seed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        return new ModuleOptions<ContentDbContext>
        {
            ModuleName = ContentConstants.ModuleName,
            SchemaName = ContentConstants.SchemaName,
            UseNoTrackingByDefault = true,
        };
    }

    /// <summary>
    /// Adds the Content module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates and seeds.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddContentModule(this IServiceCollection services, IHostEnvironment environment)
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
        services.AddScoped<LyricsInteractionErrorMessage>();
        services.AddScoped<PromotionLevelErrorMessage>();
        services.AddScoped<ArtistErrorMessage>();
        services.AddScoped<AlbumErrorMessage>();
        services.AddScoped<TranslationErrorMessage>();
        services.AddScoped<SubmissionErrorMessage>();
        services.AddScoped<LyricsRevisionErrorMessage>();
        services.AddScoped<StreamingLinkErrorMessage>();
        services.AddScoped<ShareErrorMessage>();
        services.AddSingleton<IExceptionStrategy, StreamingLinkResolutionExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, DomainRuleExceptionStrategy>();

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
        services.AddScoped<LyricsInteractionErrors>();
        services.AddScoped<PromotionLevelErrors>();
        services.AddScoped<ArtistErrors>();
        services.AddScoped<AlbumErrors>();
        services.AddScoped<TranslationErrors>();
        services.AddScoped<SubmissionErrors>();
        services.AddScoped<LyricsRevisionErrors>();
        services.AddScoped<StreamingLinkErrors>();
        services.AddScoped<ContentI18n>();

        // Contribute Content mappings to the shared cross-module Mapster config
        services.AddModuleMappings(new MappingRegistration());

        services.AddScoped<IContentUnitOfWork, ContentUnitOfWork>();
        services.AddScoped(typeof(IContentRepository<>), typeof(ContentRepository<>));

        // Replay delivers events raised inside a transaction, not just retries failed dispatches.
        services.AddScheduledJob<ContentOutboxReplayJob>(cronExpression: "0 */1 * * * ?");
        services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
        services.AddScoped<IPricingTierRepository, PricingTierRepository>();
        services.AddScoped<IPromotionLevelRepository, PromotionLevelRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleCommentRepository, ArticleCommentRepository>();
        services.AddScoped<IArticleInteractionRepository, ArticleInteractionRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IShortVideoRepository, ShortVideoRepository>();
        services.AddScoped<ILyricsRepository, LyricsRepository>();
        services.AddScoped<IContentOrderRepository, ContentOrderRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<IStreamingLinkRepository, StreamingLinkRepository>();
        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<ITranslationRevisionRepository, TranslationRevisionRepository>();
        services.AddScoped<ITranslationVoteRepository, TranslationVoteRepository>();
        services.AddScoped<ILyricsSubmissionRepository, LyricsSubmissionRepository>();
        services.AddScoped<ILyricsRevisionRepository, LyricsRevisionRepository>();
        services.AddScoped<ILyricsRevisionVoteRepository, LyricsRevisionVoteRepository>();
        services.AddScoped<IArtistClaimRequestRepository, ArtistClaimRequestRepository>();

        services.AddScoped<ICommerceCustomerNotifier, CommerceCustomerNotifier>();

        // Commerce domain event handlers
        services.AddScoped<IDomainEventHandler<OrderSubmittedEvent>, OrderSubmittedInvoiceEmailHandler>();
        services.AddScoped<IDomainEventHandler<OrderPaidEvent>, OrderPaidEffectsHandler>();
        services.AddScoped<IDomainEventHandler<OrderPaidEvent>, OrderPaidReceiptEmailHandler>();
        services.AddScoped<IDomainEventHandler<PaymentRejectedEvent>, PaymentRejectedEmailHandler>();
        services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, OrderCancelledEmailHandler>();
        services.AddScoped<IDomainEventHandler<ContentPromotionRemovedEvent>, ContentPromotionRemovedEmailHandler>();
        services.AddScoped<
            IDomainEventHandler<CommissionedContentPublishedEvent>,
            CommissionedContentPublishedEmailHandler
        >();
        services.AddScoped<
            IDomainEventHandler<CommissionedContentRejectedEvent>,
            CommissionedContentRejectedEmailHandler
        >();
        services.AddScoped<IDomainEventHandler<VideoShootScheduledEvent>, VideoShootScheduledEmailHandler>();

        // Cache invalidation domain event handlers
        services.AddScoped<IDomainEventHandler<ContentTypeChangedEvent>, LookupCacheHandler>();
        services.AddScoped<IDomainEventHandler<PricingTierChangedEvent>, LookupCacheHandler>();
        services.AddScoped<IDomainEventHandler<PromotionLevelChangedEvent>, LookupCacheHandler>();
        services.AddScoped<IDomainEventHandler<CategoryChangedEvent>, LookupCacheHandler>();
        services.AddScoped<IDomainEventHandler<ArticlePublishedEvent>, PopularArticlesCacheHandler>();
        services.AddScoped<IDomainEventHandler<ArticleUnpublishedEvent>, PopularArticlesCacheHandler>();
        services.AddScoped<IDomainEventHandler<ArticleDeletedEvent>, PopularArticlesCacheHandler>();
        services.AddScoped<IDomainEventHandler<VideoPublishedEvent>, PopularVideosCacheHandler>();
        services.AddScoped<IDomainEventHandler<VideoUnpublishedEvent>, PopularVideosCacheHandler>();
        services.AddScoped<IDomainEventHandler<VideoDeletedEvent>, PopularVideosCacheHandler>();
        services.AddScoped<IDomainEventHandler<TagGraphChangedEvent>, PopularTagsCacheHandler>();
        services.AddScoped<IDomainEventHandler<ShortVideoChangedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<ShortVideoDeletedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<ArtistChangedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<ArtistOwnershipVerifiedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<LyricsRevisionDecidedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<TranslationRevisionDecidedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<CommissionedContentPublishedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<CommissionedContentRejectedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<ContentPromotionRemovedEvent>, ContentFeedCacheHandler>();
        services.AddScoped<IDomainEventHandler<OrderPaidEvent>, ContentFeedCacheHandler>();

        // External-asset cleanup domain event handlers
        services.AddScoped<IDomainEventHandler<ArticleDeletedEvent>, ContentAssetCleanupHandler>();
        services.AddScoped<IDomainEventHandler<VideoDeletedEvent>, ContentAssetCleanupHandler>();
        services.AddScoped<IDomainEventHandler<ShortVideoDeletedEvent>, ContentAssetCleanupHandler>();
        services.AddScoped<IDomainEventHandler<ArticleBodyImagesOrphanedEvent>, ContentAssetCleanupHandler>();

        // Post-commit YouTube thumbnail acquisition
        services.AddScoped<
            IDomainEventHandler<VideoYoutubeUrlAttachedEvent>,
            VideoYoutubeUrlAttachedThumbnailHandler
        >();

        // Engagement counter domain event handlers
        services.AddScoped<IDomainEventHandler<ArticleEngagedEvent>, ArticleEngagementHandler>();
        services.AddScoped<IDomainEventHandler<VideoEngagedEvent>, VideoEngagementHandler>();
        services.AddScoped<IDomainEventHandler<LyricsEngagedEvent>, LyricsEngagementHandler>();
        services.AddScoped<IDomainEventHandler<ShortVideoEngagedEvent>, ShortVideoEngagementHandler>();
        services.AddScoped<IDomainEventHandler<CommentEngagedEvent>, CommentEngagementHandler>();

        // Community decision and reply domain event handlers. ArtistClaimRequestedEvent has no
        // consumer in v1: the durable claim-request row is the record, and the admin review
        // queue that would consume the event is future work.
        services.AddScoped<
            IDomainEventHandler<LyricsRevisionDecidedEvent>,
            LyricsRevisionDecidedNotificationsHandler
        >();
        services.AddScoped<
            IDomainEventHandler<TranslationRevisionDecidedEvent>,
            TranslationRevisionDecidedNotificationsHandler
        >();
        services.AddScoped<
            IDomainEventHandler<LyricsSubmissionDecidedEvent>,
            LyricsSubmissionDecidedNotificationsHandler
        >();
        services.AddScoped<
            IDomainEventHandler<ArtistOwnershipVerifiedEvent>,
            ArtistOwnershipVerifiedNotificationsHandler
        >();
        services.AddScoped<IDomainEventHandler<CommentReplyAddedEvent>, CommentReplyAddedNotificationsHandler>();

        // Commerce factories
        services.AddScoped<IOrderPaymentFactory, OrderPaymentFactory>();
        services.AddScoped<IVerifyPaymentFactory, AdminVerifyPaymentFactory>();
        services.AddScoped<ISubmitOrderFactory, AdminSubmitOrderFactory>();
        services.AddScoped<IAddOrderItemFactory, AdminAddOrderItemFactory>();
        services.AddScoped<IAddItemTierFactory, AdminAddItemTierFactory>();
        services.AddScoped<ICreateOrderFactory, AdminCreateOrderFactory>();

        services
            .AddHttpClient<IYoutubeThumbnailService, YoutubeThumbnailService>()
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddScoped<ITranslationService, PlaceholderTranslationService>();
        services
            .AddHttpClient<IStreamingLinkResolutionService, OdesliStreamingLinkResolutionService>()
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddScheduledJob<AbandonedDraftCleanupJob>(cronExpression: "0 0 * * * ?");
        services.AddScheduledJob<ShortVideoViewEventCleanupJob>(cronExpression: "0 0 3 * * ?");

        // Seeders run from the advisory-locked seeding hosted service. The concrete type stays
        // registered for direct resolution; Testing hosts register no IDataSeeder, so the
        // hosted service is a no-op there.
        services.AddScoped<ContentTypeSeeder>();
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddScoped<IDataSeeder>(sp => sp.GetRequiredService<ContentTypeSeeder>());
        }

        return services;
    }
}
