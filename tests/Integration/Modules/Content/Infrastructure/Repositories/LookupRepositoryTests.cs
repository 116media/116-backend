using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="ILookupRepository"/> verifying persistence behavior
/// for ContentType, PricingTier, PromotionLevel, and Tag lookup entities
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class LookupRepositoryTests : BaseRepositoryTest
{
    public LookupRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetContentTypeByIdOrThrowAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("TestArticle");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetContentTypeByIdOrThrowAsync(contentType.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(contentType.Id);
        result.Name.Should().Be("TestArticle");
    }

    [Fact]
    public async Task GetContentTypeByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<ILookupRepository>();

        var act = () => repo.GetContentTypeByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ContentTypeExistsByNameAsync_WhenExists_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create("UniqueContentType");
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var exists = await repo.ContentTypeExistsByNameAsync("UniqueContentType");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ContentTypeExistsByNameAsync_WhenNotFound_ReturnsFalse()
    {
        var repo = Resolve<ILookupRepository>();

        var exists = await repo.ContentTypeExistsByNameAsync("NonExistentContentType");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllContentTypesAsync_WithoutSearch_ReturnsAllOrderedByName()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.ContentTypes.AddRange(
            ContentTypeFactory.Create("Zeta"),
            ContentTypeFactory.Create("Alpha"),
            ContentTypeFactory.Create("Mango")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllContentTypesAsync();

        result.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Should().BeInAscendingOrder(x => x.Name);
    }

    [Fact]
    public async Task GetAllContentTypesAsync_WithSearch_FiltersResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.ContentTypes.AddRange(
            ContentTypeFactory.Create("SearchableType"),
            ContentTypeFactory.Create("OtherType")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllContentTypesAsync(search: "Searchable");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("SearchableType");
    }

    [Fact]
    public async Task GetActiveContentTypesAsync_ReturnsOnlyActiveEntities()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = ContentTypeFactory.Create("ActiveType");
        var inactive = ContentTypeFactory.CreateInactive();
        seedContext.ContentTypes.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetActiveContentTypesAsync();

        result.Should().OnlyContain(x => x.IsActive);
        result.Should().Contain(x => x.Id == active.Id);
        result.Should().NotContain(x => x.Id == inactive.Id);
    }

    [Fact]
    public async Task GetPricingTierByIdOrThrowAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tier = PricingTierFactory.Create("Premium");
        seedContext.PricingTiers.Add(tier);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetPricingTierByIdOrThrowAsync(tier.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(tier.Id);
        result.Name.Should().Be("Premium");
    }

    [Fact]
    public async Task GetPricingTierByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<ILookupRepository>();

        var act = () => repo.GetPricingTierByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllPricingTiersAsync_WithSearch_FiltersResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        seedContext.PricingTiers.AddRange(
            PricingTierFactory.Create("GoldTier"),
            PricingTierFactory.Create("SilverTier"),
            PricingTierFactory.Create("Bronze")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllPricingTiersAsync(search: "Gold");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("GoldTier");
    }

    [Fact]
    public async Task PromotionLevelExistsByNameAsync_WhenExists_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var level = PromotionLevelFactory.Create("Featured", 30, 49.99m);
        seedContext.PromotionLevels.Add(level);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var exists = await repo.PromotionLevelExistsByNameAsync("Featured");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetActivePromotionLevelsAsync_ReturnsOnlyActiveEntities()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = PromotionLevelFactory.Create("ActivePromo", 7, 9.99m);
        var inactive = PromotionLevelFactory.CreateInactive();
        seedContext.PromotionLevels.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetActivePromotionLevelsAsync();

        result.Should().OnlyContain(x => x.IsActive);
        result.Should().Contain(x => x.Id == active.Id);
        result.Should().NotContain(x => x.Id == inactive.Id);
    }

    [Fact]
    public async Task GetTagByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<ILookupRepository>();

        var act = () => repo.GetTagByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTagBySlugAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Afrobeats", "afrobeats");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetTagBySlugAsync("afrobeats");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("afrobeats");
        result.Name.Should().Be("Afrobeats");
    }

    [Fact]
    public async Task GetTagBySlugAsync_WhenNotFound_ReturnsNull()
    {
        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetTagBySlugAsync("non-existent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTagByNameAsync_WhenExists_ReturnsCaseInsensitiveMatch()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Kinshasa", "kinshasa");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetTagByNameAsync("kinshasa");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Kinshasa");
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithLimit_RespectsLimit()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tags = TagFactory.CreateMany(5);
        seedContext.Tags.AddRange(tags);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetPopularTagsAsync(limit: 3);

        result.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllTagsAsync_WithArticleContentType_ReturnsOnlyArticleAssociatedTags()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.Create(category.Id);
        var video = VideoFactory.Create(category.Id);
        var articleTag = TagFactory.Create("AllTagsArticleTag", "all-tags-article-tag");
        var videoTag = TagFactory.Create("AllTagsVideoTag", "all-tags-video-tag");
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        seedContext.Videos.Add(video);
        seedContext.Tags.AddRange(articleTag, videoTag);
        seedContext.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, articleTag.Id));
        seedContext.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), video.Id, videoTag.Id));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllTagsAsync(search: null, contentType: EnumCoreContentType.Article);

        result.Should().Contain(t => t.Id == articleTag.Id);
        result.Should().NotContain(t => t.Id == videoTag.Id);
    }

    [Fact]
    public async Task GetAllTagsAsync_WithVideoContentType_ReturnsOnlyVideoAssociatedTags()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.Create(category.Id);
        var video = VideoFactory.Create(category.Id);
        var articleTag = TagFactory.Create("AllTagsArticleOnly", "all-tags-article-only");
        var videoTag = TagFactory.Create("AllTagsVideoOnly", "all-tags-video-only");
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        seedContext.Videos.Add(video);
        seedContext.Tags.AddRange(articleTag, videoTag);
        seedContext.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, articleTag.Id));
        seedContext.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), video.Id, videoTag.Id));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllTagsAsync(search: null, contentType: EnumCoreContentType.Video);

        result.Should().Contain(t => t.Id == videoTag.Id);
        result.Should().NotContain(t => t.Id == articleTag.Id);
    }

    [Fact]
    public async Task GetAllTagsAsync_WithSearchAndContentType_ComposesBothFilters()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.Create(category.Id);
        var video = VideoFactory.Create(category.Id);
        var matching = TagFactory.Create("ComposeAfrobeats", "compose-afrobeats");
        var searchMismatch = TagFactory.Create("ComposeReggae", "compose-reggae");
        var associationMismatch = TagFactory.Create("ComposeAfropop", "compose-afropop");
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        seedContext.Videos.Add(video);
        seedContext.Tags.AddRange(matching, searchMismatch, associationMismatch);
        seedContext.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, matching.Id));
        seedContext.ArticleTags.Add(ArticleTagEntity.Create(Guid.NewGuid(), article.Id, searchMismatch.Id));
        seedContext.VideoTags.Add(VideoTagEntity.Create(Guid.NewGuid(), video.Id, associationMismatch.Id));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ILookupRepository>();

        var result = await repo.GetAllTagsAsync(search: "Afro", contentType: EnumCoreContentType.Article);

        result.Should().Contain(t => t.Id == matching.Id);
        result.Should().NotContain(t => t.Id == searchMismatch.Id);
        result.Should().NotContain(t => t.Id == associationMismatch.Id);
    }

    [Fact]
    public async Task AddTagAsync_PersistsTagToDatabase()
    {
        var (repo, db) = CreateScopedRepository<ILookupRepository, ContentDbContext>();
        var tag = TagFactory.Create("NewTag", "new-tag");

        await repo.AddTagAsync(tag);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("NewTag");
        persisted.Slug.Should().Be("new-tag");
    }
}
