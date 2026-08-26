using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="ITagRepository"/> verifying persistence behavior against a real
/// PostgreSQL database.
/// </summary>
[Collection("Database")]
public class TagRepositoryTests : BaseRepositoryTest
{
    public TagRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<ITagRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Afrobeats", "afrobeats");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetBySlugAsync("afrobeats");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("afrobeats");
        result.Name.Should().Be("Afrobeats");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ReturnsNull()
    {
        var repo = Resolve<ITagRepository>();

        var result = await repo.GetBySlugAsync("non-existent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_WhenExists_ReturnsCaseInsensitiveMatch()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tag = TagFactory.Create("Kinshasa", "kinshasa");
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetByNameAsync("kinshasa");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Kinshasa");
    }

    [Fact]
    public async Task GetPopularAsync_WithLimit_RespectsLimit()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var tags = TagFactory.CreateMany(5);
        seedContext.Tags.AddRange(tags);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetPopularAsync(limit: 3);

        result.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public async Task AddAsync_PersistsTagToDatabase()
    {
        var (repo, db) = CreateScopedRepository<ITagRepository, ContentDbContext>();
        var tag = TagFactory.Create("NewTag", "new-tag");

        await repo.AddAsync(tag);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Tags.FirstOrDefaultAsync(t => t.Id == tag.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("NewTag");
        persisted.Slug.Should().Be("new-tag");
    }

    [Fact]
    public async Task GetAllAsync_WithArticleContentType_ReturnsOnlyArticleAssociatedTags()
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

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetAllAsync(search: null, contentType: EnumCoreContentType.Article);

        result.Should().Contain(t => t.Id == articleTag.Id);
        result.Should().NotContain(t => t.Id == videoTag.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithVideoContentType_ReturnsOnlyVideoAssociatedTags()
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

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetAllAsync(search: null, contentType: EnumCoreContentType.Video);

        result.Should().Contain(t => t.Id == videoTag.Id);
        result.Should().NotContain(t => t.Id == articleTag.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchAndContentType_ComposesBothFilters()
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

        var repo = Resolve<ITagRepository>();

        var result = await repo.GetAllAsync(search: "Afro", contentType: EnumCoreContentType.Article);

        result.Should().Contain(t => t.Id == matching.Id);
        result.Should().NotContain(t => t.Id == searchMismatch.Id);
        result.Should().NotContain(t => t.Id == associationMismatch.Id);
    }
}
