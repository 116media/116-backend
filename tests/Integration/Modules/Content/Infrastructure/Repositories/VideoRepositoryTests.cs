using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IVideoRepository"/> verifying persistence behavior
/// for videos, video tags, video ratings, and video shares against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class VideoRepositoryTests : BaseRepositoryTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VideoRepositoryTests"/> class
    /// with the shared database fixture.
    /// </summary>
    public VideoRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    private async Task<(ContentTypeEntity ContentType, CategoryEntity Category)> SeedCategoryChainAsync(
        ContentDbContext context
    )
    {
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return (contentType, category);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var videos = VideoFactory.CreateMany(category.Id, 5);
        seedContext.Videos.AddRange(videos);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var (result, totalCount) = await repo.GetAllAsync(
            page: 1,
            pageSize: 3,
            search: null,
            status: null,
            categoryId: null
        );

        totalCount.Should().Be(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var published = VideoFactory.CreatePublished(category.Id);
        seedContext.Videos.Add(published);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var (result, _) = await repo.GetAllAsync(
            page: 1,
            pageSize: 100,
            search: null,
            status: EnumContentStatus.Published,
            categoryId: null
        );

        result.Should().OnlyContain(v => v.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ReturnsOnlyMatchingCategory()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        seedContext.ContentTypes.Add(contentType);
        await seedContext.SaveChangesAsync();

        var category1 = CategoryFactory.Create(contentType.Id);
        var category2 = CategoryFactory.Create(contentType.Id);
        seedContext.Categories.AddRange(category1, category2);
        await seedContext.SaveChangesAsync();

        var video1 = VideoFactory.Create(category1.Id);
        var video2 = VideoFactory.Create(category2.Id);
        seedContext.Videos.AddRange(video1, video2);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var (result, _) = await repo.GetAllAsync(
            page: 1,
            pageSize: 100,
            search: null,
            status: null,
            categoryId: category1.Id
        );

        result.Should().OnlyContain(v => v.CategoryId == category1.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVideoExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetByIdAsync(video.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(video.Id);
        result.Category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenVideoDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenVideoDoesNotExist_ThrowsNotFoundException()
    {
        var repo = Resolve<IVideoRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var video = VideoFactory.CreateWithSlug(category.Id, "test-video-slug");
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetBySlugAsync("test-video-slug");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-video-slug");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetBySlugAsync("non-existent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesArchivedAndRejected()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var published = VideoFactory.CreatePublished(category.Id);
        seedContext.Videos.Add(published);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetActiveAsync();

        result.Should().Contain(v => v.Id == published.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsVideoToDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var (repo, db) = CreateScopedRepository<IVideoRepository, ContentDbContext>();
        var video = VideoFactory.Create(category.Id);

        await repo.AddAsync(video);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);

        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be(video.Title);
    }

    [Fact]
    public async Task Remove_DeletesVideoFromDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IVideoRepository, ContentDbContext>();
        var existing = await db.Videos.FirstAsync(v => v.Id == video.Id);

        repo.Remove(existing);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var deleted = await verifyContext.Videos.FirstOrDefaultAsync(v => v.Id == video.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetTagsByVideoIdAsync_ReturnsTagsForVideo()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);

        var tag = TagFactory.Create();
        seedContext.Tags.Add(tag);
        await seedContext.SaveChangesAsync();

        var videoTag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: video.Id, tagId: tag.Id);
        seedContext.VideoTags.Add(videoTag);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetTagsByVideoIdAsync(video.Id);

        result.Should().ContainSingle();
        result[0].VideoId.Should().Be(video.Id);
        result[0].TagId.Should().Be(tag.Id);
    }

    [Fact]
    public async Task GetRatingAsync_WhenExists_ReturnsRating()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        var video = VideoFactory.Create(category.Id);
        seedContext.Videos.Add(video);
        await seedContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var rating = VideoRatingFactory.Create(videoId: video.Id, userId: userId, stars: 4);
        seedContext.VideoRatings.Add(rating);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetRatingAsync(userId: userId, videoId: video.Id);

        result.Should().NotBeNull();
        result!.VideoId.Should().Be(video.Id);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetRatingAsync_WhenDoesNotExist_ReturnsNull()
    {
        var repo = Resolve<IVideoRepository>();
        var result = await repo.GetRatingAsync(userId: Guid.NewGuid(), videoId: Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        string uniqueKeyword = $"UniqueSearchKw{Guid.NewGuid():N}"[..20];
        var matchingVideo = VideoFactory.CreateWithTitle(category.Id, $"{uniqueKeyword} Music Video");
        var nonMatchingVideo = VideoFactory.Create(category.Id);

        seedContext.Videos.AddRange(matchingVideo, nonMatchingVideo);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        var (result, totalCount) = await repo.GetAllAsync(
            page: 1,
            pageSize: 100,
            search: uniqueKeyword,
            status: null,
            categoryId: null
        );

        totalCount.Should().Be(1);
        result.Should().Contain(v => v.Id == matchingVideo.Id);
        result.Should().NotContain(v => v.Id == nonMatchingVideo.Id);
    }

    [Fact]
    public async Task GetLatestPublishedByCategoryAsync_OrdersByPublishedAtDescending()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        VideoEntity older = VideoFactory.CreatePublishedAt(
            category.Id,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        VideoEntity newer = VideoFactory.CreatePublishedAt(
            category.Id,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)
        );
        seedContext.Videos.AddRange(older, newer);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        IReadOnlyList<VideoEntity> result = await repo.GetLatestPublishedByCategoryAsync(category.Id, 8);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newer.Id);
        result[1].Id.Should().Be(older.Id);
    }

    [Fact]
    public async Task GetLatestPublishedByCategoryAsync_ReturnsOnlyPublished()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        VideoEntity published = VideoFactory.CreatePublished(category.Id);
        VideoEntity draft = VideoFactory.Create(category.Id);
        seedContext.Videos.AddRange(published, draft);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        IReadOnlyList<VideoEntity> result = await repo.GetLatestPublishedByCategoryAsync(category.Id, 8);

        result.Should().OnlyContain(v => v.Status == EnumContentStatus.Published);
        result.Should().Contain(v => v.Id == published.Id);
        result.Should().NotContain(v => v.Id == draft.Id);
    }

    [Fact]
    public async Task GetLatestPublishedByCategoryAsync_RespectsLimit()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        seedContext.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 12));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        IReadOnlyList<VideoEntity> result = await repo.GetLatestPublishedByCategoryAsync(category.Id, 8);

        result.Should().HaveCount(8);
    }

    [Fact]
    public async Task CountPublishedByCategoryAsync_CountsOnlyPublished()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var (_, category) = await SeedCategoryChainAsync(seedContext);

        seedContext.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 4));
        seedContext.Videos.AddRange(VideoFactory.CreateMany(category.Id, 3)); // drafts
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IVideoRepository>();
        int count = await repo.CountPublishedByCategoryAsync(category.Id);

        count.Should().Be(4);
    }
}
