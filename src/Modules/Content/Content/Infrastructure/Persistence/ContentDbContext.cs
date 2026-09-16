using System.Reflection;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace _116.Content.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Content module.
/// Manages all content-related entities under the "content" schema.
/// </summary>
/// <param name="options">The options to configure this database context.</param>
public class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the DbSet for content type entities.
    /// </summary>
    public DbSet<ContentTypeEntity> ContentTypes => Set<ContentTypeEntity>();

    /// <summary>
    /// Gets the DbSet for pricing tier entities.
    /// </summary>
    public DbSet<PricingTierEntity> PricingTiers => Set<PricingTierEntity>();

    /// <summary>
    /// Gets the DbSet for promotion level entities.
    /// </summary>
    public DbSet<PromotionLevelEntity> PromotionLevels => Set<PromotionLevelEntity>();

    /// <summary>
    /// Gets the DbSet for tag entities.
    /// </summary>
    public DbSet<TagEntity> Tags => Set<TagEntity>();

    /// <summary>
    /// Gets the DbSet for category entities.
    /// </summary>
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    /// <summary>
    /// Gets the DbSet for category pricing entities.
    /// </summary>
    public DbSet<CategoryPricingEntity> CategoryPricing => Set<CategoryPricingEntity>();

    /// <summary>
    /// Gets the DbSet for customer entities.
    /// </summary>
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

    /// <summary>
    /// Gets the DbSet for package entities.
    /// </summary>
    public DbSet<PackageEntity> Packages => Set<PackageEntity>();

    /// <summary>
    /// Gets the DbSet for package slot entities.
    /// </summary>
    public DbSet<PackageSlotEntity> PackageSlots => Set<PackageSlotEntity>();

    /// <summary>
    /// Gets the DbSet for article entities.
    /// </summary>
    public DbSet<ArticleEntity> Articles => Set<ArticleEntity>();

    /// <summary>
    /// Gets the DbSet for article image entities (tracks all cover and body images per article).
    /// </summary>
    public DbSet<ArticleImageEntity> ArticleImages => Set<ArticleImageEntity>();

    /// <summary>
    /// Gets the DbSet for article tag junction entities.
    /// </summary>
    public DbSet<ArticleTagEntity> ArticleTags => Set<ArticleTagEntity>();

    /// <summary>
    /// Gets the DbSet for video entities.
    /// </summary>
    public DbSet<VideoEntity> Videos => Set<VideoEntity>();

    /// <summary>
    /// Gets the DbSet for video tag junction entities.
    /// </summary>
    public DbSet<VideoTagEntity> VideoTags => Set<VideoTagEntity>();

    /// <summary>
    /// Gets the DbSet for short video entities.
    /// </summary>
    public DbSet<ShortVideoEntity> ShortVideos => Set<ShortVideoEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics entities.
    /// </summary>
    public DbSet<LyricsEntity> Lyrics => Set<LyricsEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics tag junction entities.
    /// </summary>
    public DbSet<LyricsTagEntity> LyricsTags => Set<LyricsTagEntity>();

    /// <summary>
    /// Gets the DbSet for content order entities.
    /// </summary>
    public DbSet<ContentOrderEntity> ContentOrders => Set<ContentOrderEntity>();

    /// <summary>
    /// Gets the DbSet for content order item entities.
    /// </summary>
    public DbSet<ContentOrderItemEntity> ContentOrderItems => Set<ContentOrderItemEntity>();

    /// <summary>
    /// Gets the DbSet for content item tier snapshot entities.
    /// </summary>
    public DbSet<ContentItemTierEntity> ContentItemTiers => Set<ContentItemTierEntity>();

    /// <summary>
    /// Gets the DbSet for content payment entities.
    /// </summary>
    public DbSet<ContentPaymentEntity> ContentPayments => Set<ContentPaymentEntity>();

    /// <summary>
    /// Gets the DbSet for article like entities.
    /// </summary>
    public DbSet<ArticleLikeEntity> ArticleLikes => Set<ArticleLikeEntity>();

    /// <summary>
    /// Gets the DbSet for article bookmark entities.
    /// </summary>
    public DbSet<ArticleBookmarkEntity> ArticleBookmarks => Set<ArticleBookmarkEntity>();

    /// <summary>
    /// Gets the DbSet for article share entities.
    /// </summary>
    public DbSet<ArticleShareEntity> ArticleShares => Set<ArticleShareEntity>();

    /// <summary>
    /// Gets the DbSet for article comment entities.
    /// </summary>
    public DbSet<ArticleCommentEntity> ArticleComments => Set<ArticleCommentEntity>();

    /// <summary>
    /// Gets the DbSet for article comment like entities.
    /// </summary>
    public DbSet<ArticleCommentLikeEntity> ArticleCommentLikes => Set<ArticleCommentLikeEntity>();

    /// <summary>
    /// Gets the DbSet for video rating entities.
    /// </summary>
    public DbSet<VideoRatingEntity> VideoRatings => Set<VideoRatingEntity>();

    /// <summary>
    /// Gets the DbSet for video share entities.
    /// </summary>
    public DbSet<VideoShareEntity> VideoShares => Set<VideoShareEntity>();

    /// <summary>
    /// Gets the DbSet for playlist entities.
    /// </summary>
    public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();

    /// <summary>
    /// Gets the DbSet for playlist video junction entities.
    /// </summary>
    public DbSet<PlaylistVideoEntity> PlaylistVideos => Set<PlaylistVideoEntity>();

    /// <summary>
    /// Gets the DbSet for short video like entities.
    /// </summary>
    public DbSet<ShortVideoLikeEntity> ShortVideoLikes => Set<ShortVideoLikeEntity>();

    /// <summary>
    /// Gets the DbSet for short video bookmark entities.
    /// </summary>
    public DbSet<ShortVideoBookmarkEntity> ShortVideoBookmarks => Set<ShortVideoBookmarkEntity>();

    /// <summary>
    /// Gets the DbSet for short video share entities.
    /// </summary>
    public DbSet<ShortVideoShareEntity> ShortVideoShares => Set<ShortVideoShareEntity>();

    /// <summary>
    /// Gets the DbSet for raw short video view event entities.
    /// </summary>
    public DbSet<ShortVideoViewEventEntity> ShortVideoViewEvents => Set<ShortVideoViewEventEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics like entities.
    /// </summary>
    public DbSet<LyricsLikeEntity> LyricsLikes => Set<LyricsLikeEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics share entities.
    /// </summary>
    public DbSet<LyricsShareEntity> LyricsShares => Set<LyricsShareEntity>();

    /// <summary>
    /// Gets the DbSet for raw lyrics view event entities.
    /// </summary>
    public DbSet<LyricsViewEventEntity> LyricsViewEvents => Set<LyricsViewEventEntity>();

    /// <summary>
    /// Gets the DbSet for artist profile entities.
    /// </summary>
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();

    /// <summary>
    /// Gets the DbSet for artist social link entities.
    /// </summary>
    public DbSet<ArtistSocialLinkEntity> ArtistSocialLinks => Set<ArtistSocialLinkEntity>();

    /// <summary>
    /// Gets the DbSet for article-artist junction entities.
    /// </summary>
    public DbSet<ArticleArtistEntity> ArticleArtists => Set<ArticleArtistEntity>();

    /// <summary>
    /// Gets the DbSet for album entities.
    /// </summary>
    public DbSet<AlbumEntity> Albums => Set<AlbumEntity>();

    /// <summary>
    /// Gets the DbSet for streaming link entities.
    /// </summary>
    public DbSet<StreamingLinkEntity> StreamingLinks => Set<StreamingLinkEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics translation entities.
    /// </summary>
    public DbSet<LyricsTranslationEntity> LyricsTranslations => Set<LyricsTranslationEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics translation revision entities.
    /// </summary>
    public DbSet<LyricsTranslationRevisionEntity> LyricsTranslationRevisions => Set<LyricsTranslationRevisionEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics translation vote entities.
    /// </summary>
    public DbSet<LyricsTranslationVoteEntity> LyricsTranslationVotes => Set<LyricsTranslationVoteEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics submission entities.
    /// </summary>
    public DbSet<LyricsSubmissionEntity> LyricsSubmissions => Set<LyricsSubmissionEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics revision entities.
    /// </summary>
    public DbSet<LyricsRevisionEntity> LyricsRevisions => Set<LyricsRevisionEntity>();

    /// <summary>
    /// Gets the DbSet for lyrics revision vote entities.
    /// </summary>
    public DbSet<LyricsRevisionVoteEntity> LyricsRevisionVotes => Set<LyricsRevisionVoteEntity>();

    /// <summary>
    /// Gets the DbSet for artist ownership claim request entities.
    /// </summary>
    public DbSet<ArtistClaimRequestEntity> ArtistClaimRequests => Set<ArtistClaimRequestEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ContentConstants.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new OutboxEventConfiguration());

        // Every identifier is assigned by the domain factory, never by the store. Saying so
        // is what lets EF treat a member added to a root's collection as an insert; left as
        // store-generated, a child arriving with its key already set is tracked as Modified
        // and its INSERT never runs.
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IMutableProperty? key = entityType.FindPrimaryKey()?.Properties.SingleOrDefault();

            if (key?.ClrType == typeof(Guid))
            {
                key.ValueGenerated = ValueGenerated.Never;
            }
        }

        // Soft-deleted comments are invisible by default; the threaded listing opts back in
        // with IgnoreQueryFilters because it renders tombstones for reply continuity.
        modelBuilder.Entity<ArticleCommentEntity>().HasQueryFilter(comment => !comment.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
