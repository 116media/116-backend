using _116.BuildingBlocks.Application.Services;
using _116.BuildingBlocks.Infrastructure;
using _116.BuildingBlocks.Infrastructure.Seed;
using _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.Contracts;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.Contracts;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.Contracts;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.Contracts;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.Contracts;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.Contracts;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.Contracts;
using _116.Content.Application.Commerce.EventHandlers;
using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Editorial.Ports;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateVideo.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.Contracts;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;
using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug.Contracts;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;
using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed.Contracts;
using _116.Content.Application.Interactions.EventHandlers;
using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.Services;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Exceptions.Handlers;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Ports;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Content.Infrastructure.Repositories;
using _116.Content.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.Content.Infrastructure;

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
        services.AddHttpCurrentActor();
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
        services.AddScoped<ValueObjectErrorMessage>();
        services.AddSingleton<IExceptionStrategy, StreamingLinkResolutionExceptionHandler>();
        services.AddSingleton<IExceptionStrategy, DomainRuleExceptionStrategy>();

        // Register error service classes
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

        // Commerce services
        services.AddScoped<IOrderPaymentService, OrderPaymentService>();
        services.AddScoped<IVerifyPaymentService, AdminVerifyPaymentService>();
        services.AddScoped<ISubmitOrderService, AdminSubmitOrderService>();
        services.AddScoped<IAddOrderItemService, AdminAddOrderItemService>();
        services.AddScoped<IAddItemTierService, AdminAddItemTierService>();
        services.AddScoped<ICreateOrderService, AdminCreateOrderService>();
        services.AddScoped<IContentOrderDtoService, ContentOrderDtoService>();
        services.AddScoped<IPaymentDtoService, PaymentDtoService>();

        // Catalog services
        services.AddScoped<IContentLookupService, ContentLookupService>();
        services.AddScoped<ICategoryDtoService, CategoryDtoService>();
        services.AddScoped<IPackageDtoService, PackageDtoService>();

        // Editorial services
        services.AddScoped<IArtistDtoService, ArtistDtoService>();
        services.AddScoped<IAlbumDtoService, AlbumDtoService>();
        services.AddScoped<IVideoDtoService, VideoDtoService>();

        // Interactions services
        services.AddScoped<IPlaylistDtoService, PlaylistDtoService>();

        // Application services extracted from the handlers over the dependency budget.
        services.AddScoped<ICategoryPricingDtoService, CategoryPricingDtoService>();
        services.AddScoped<IAdminAddCategoryPricingService, AdminAddCategoryPricingService>();
        services.AddScoped<IAdminAddPackageSlotService, AdminAddPackageSlotService>();
        services.AddScoped<IAdminCreateCategoryService, AdminCreateCategoryService>();
        services.AddScoped<IAdminPinCategoryToFeedService, AdminPinCategoryToFeedService>();
        services.AddScoped<IAdminSetExclusiveCategoryService, AdminSetExclusiveCategoryService>();
        services.AddScoped<IAdminUpdateCategoryService, AdminUpdateCategoryService>();
        services.AddScoped<IPublicExclusiveCategoryVideosService, PublicExclusiveCategoryVideosService>();
        services.AddScoped<IAdminEditOrderService, AdminEditOrderService>();
        services.AddScoped<IAdminEditOrderItemService, AdminEditOrderItemService>();
        services.AddScoped<ILyricsDtoService, LyricsDtoService>();
        services.AddScoped<IArticleDtoService, ArticleDtoService>();
        services.AddScoped<IShortVideoDtoService, ShortVideoDtoService>();
        services.AddScoped<IAdminApproveLyricsSubmissionService, AdminApproveLyricsSubmissionService>();
        services.AddScoped<IAdminCreateArticleService, AdminCreateArticleService>();
        services.AddScoped<IAdminCreateArtistService, AdminCreateArtistService>();
        services.AddScoped<IAdminCreateLyricsService, AdminCreateLyricsService>();
        services.AddScoped<IAdminCreateVideoService, AdminCreateVideoService>();
        services.AddScoped<IAdminForceUnpromoteArticleService, AdminForceUnpromoteArticleService>();
        services.AddScoped<IAdminForceUnpromoteVideoService, AdminForceUnpromoteVideoService>();
        services.AddScoped<IAdminAlbumLinkResolutionService, AdminAlbumLinkResolutionService>();
        services.AddScoped<IAdminSingleLinkResolutionService, AdminSingleLinkResolutionService>();
        services.AddScoped<IAdminUpdateArticleService, AdminUpdateArticleService>();
        services.AddScoped<IAdminUpdateLyricsService, AdminUpdateLyricsService>();
        services.AddScoped<IAdminUpdateVideoService, AdminUpdateVideoService>();
        services.AddScoped<IPublicSubmitLyricsService, PublicSubmitLyricsService>();
        services.AddScoped<IPublicLyricsRevisionVoteService, PublicLyricsRevisionVoteService>();
        services.AddScoped<IPublicTranslationRevisionVoteService, PublicTranslationRevisionVoteService>();
        services.AddScoped<IPublicArtistPageService, PublicArtistPageService>();
        services.AddScoped<IPublicLyricsPageService, PublicLyricsPageService>();
        services.AddScoped<IPublicVideoFeedService, PublicVideoFeedService>();
        services.AddScoped<IArticleCommentDtoService, ArticleCommentDtoService>();

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
