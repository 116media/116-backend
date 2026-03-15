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
