using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="ContentDbContext"/>.
/// </summary>
public class ContentDbContextTests
{
    private static DbContextOptions<ContentDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    #region DbSet Properties

    [Fact]
    public void ContentTypes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ContentTypeEntity> result = context.ContentTypes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void PricingTiers_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PricingTierEntity> result = context.PricingTiers;
        result.Should().NotBeNull();
    }

    [Fact]
    public void PromotionLevels_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PromotionLevelEntity> result = context.PromotionLevels;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Tags_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<TagEntity> result = context.Tags;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Categories_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<CategoryEntity> result = context.Categories;
        result.Should().NotBeNull();
    }

    [Fact]
    public void CategoryPricing_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<CategoryPricingEntity> result = context.CategoryPricing;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Customers_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<CustomerEntity> result = context.Customers;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Packages_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PackageEntity> result = context.Packages;
        result.Should().NotBeNull();
    }

    [Fact]
    public void PackageSlots_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PackageSlotEntity> result = context.PackageSlots;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Articles_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleEntity> result = context.Articles;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleImages_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleImageEntity> result = context.ArticleImages;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleTags_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleTagEntity> result = context.ArticleTags;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Videos_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<VideoEntity> result = context.Videos;
        result.Should().NotBeNull();
    }

    [Fact]
    public void VideoTags_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<VideoTagEntity> result = context.VideoTags;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ShortVideos_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ShortVideoEntity> result = context.ShortVideos;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Lyrics_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsEntity> result = context.Lyrics;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ContentOrders_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ContentOrderEntity> result = context.ContentOrders;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ContentOrderItems_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ContentOrderItemEntity> result = context.ContentOrderItems;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ContentItemTiers_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ContentItemTierEntity> result = context.ContentItemTiers;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ContentPayments_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ContentPaymentEntity> result = context.ContentPayments;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleLikes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleLikeEntity> result = context.ArticleLikes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleBookmarks_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleBookmarkEntity> result = context.ArticleBookmarks;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleShares_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleShareEntity> result = context.ArticleShares;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleComments_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleCommentEntity> result = context.ArticleComments;
        result.Should().NotBeNull();
    }

    [Fact]
    public void VideoRatings_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<VideoRatingEntity> result = context.VideoRatings;
        result.Should().NotBeNull();
    }

    [Fact]
    public void VideoShares_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<VideoShareEntity> result = context.VideoShares;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ShortVideoLikes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ShortVideoLikeEntity> result = context.ShortVideoLikes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ShortVideoBookmarks_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ShortVideoBookmarkEntity> result = context.ShortVideoBookmarks;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ShortVideoShares_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ShortVideoShareEntity> result = context.ShortVideoShares;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsTags_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsTagEntity> result = context.LyricsTags;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ArticleCommentLikes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArticleCommentLikeEntity> result = context.ArticleCommentLikes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Playlists_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PlaylistEntity> result = context.Playlists;
        result.Should().NotBeNull();
    }

    [Fact]
    public void PlaylistVideos_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<PlaylistVideoEntity> result = context.PlaylistVideos;
        result.Should().NotBeNull();
    }

    [Fact]
    public void ShortVideoViewEvents_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ShortVideoViewEventEntity> result = context.ShortVideoViewEvents;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsLikes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsLikeEntity> result = context.LyricsLikes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsShares_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsShareEntity> result = context.LyricsShares;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsViewEvents_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsViewEventEntity> result = context.LyricsViewEvents;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Artists_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<ArtistEntity> result = context.Artists;
        result.Should().NotBeNull();
    }

    [Fact]
    public void Albums_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<AlbumEntity> result = context.Albums;
        result.Should().NotBeNull();
    }

    [Fact]
    public void StreamingLinks_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<StreamingLinkEntity> result = context.StreamingLinks;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsTranslations_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsTranslationEntity> result = context.LyricsTranslations;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsTranslationRevisions_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsTranslationRevisionEntity> result = context.LyricsTranslationRevisions;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsTranslationVotes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsTranslationVoteEntity> result = context.LyricsTranslationVotes;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsSubmissions_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsSubmissionEntity> result = context.LyricsSubmissions;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsRevisions_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsRevisionEntity> result = context.LyricsRevisions;
        result.Should().NotBeNull();
    }

    [Fact]
    public void LyricsRevisionVotes_ShouldReturnDbSet()
    {
        using var context = new ContentDbContext(CreateOptions());
        DbSet<LyricsRevisionVoteEntity> result = context.LyricsRevisionVotes;
        result.Should().NotBeNull();
    }

    #endregion

    #region Schema and Configuration

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        using var context = new ContentDbContext(CreateOptions());
        IModel model = context.Model;
        IEntityType? articleEntityType = model.FindEntityType(typeof(ArticleEntity));
        articleEntityType.Should().NotBeNull();
        articleEntityType.GetSchema().Should().Be("content");
    }

    [Fact]
    public void Context_ShouldSetDefaultSchemaToContent()
    {
        using var context = new ContentDbContext(CreateOptions());
        IModel model = context.Model;
        IEntityType? videoEntityType = model.FindEntityType(typeof(VideoEntity));
        videoEntityType.Should().NotBeNull();
        videoEntityType.GetSchema().Should().Be("content");
    }

    [Fact]
    public void Context_ShouldHaveAllEntityTypesConfigured()
    {
        using var context = new ContentDbContext(CreateOptions());
        IModel model = context.Model;

        model.FindEntityType(typeof(ContentTypeEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PricingTierEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PromotionLevelEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(TagEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CategoryEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CategoryPricingEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(CustomerEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PackageEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PackageSlotEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(ArticleEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(ArticleImageEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(ArticleTagEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(VideoEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(VideoTagEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(ShortVideoEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(LyricsEntity)).Should().NotBeNull();
    }

    #endregion
}
