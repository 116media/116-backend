using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IShortVideoRepository"/> verifying short video CRUD,
/// pagination, like/bookmark/share operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class ShortVideoRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyWhenNoShortVideosExist()
    {
        var repo = Resolve<IShortVideoRepository>();

        var (shortVideos, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 10, search: null, isActive: null);

        totalCount.Should().Be(0);
        shortVideos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var videos = ShortVideoFactory.CreateMany(5);
        seedContext.ShortVideos.AddRange(videos);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var (result, totalCount) = await repo.GetAllAsync(page: 1, pageSize: 3, search: null, isActive: null);

        totalCount.Should().BeGreaterThanOrEqualTo(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_ReturnsOnlyMatchingVideos()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = ShortVideoFactory.Create();
        var inactive = ShortVideoFactory.CreateInactive();
        seedContext.ShortVideos.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var (result, _) = await repo.GetAllAsync(page: 1, pageSize: 50, search: null, isActive: true);

        result.Should().OnlyContain(v => v.IsActive);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenExists_ReturnsEntity()
    {
        var slug = $"test-slug-{Guid.NewGuid():N}";
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var video = ShortVideoFactory.CreateWithSlug(slug);
        seedContext.ShortVideos.Add(video);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.GetBySlugAsync(slug);

        result.Should().NotBeNull();
        result!.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ReturnsNull()
    {
        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.GetBySlugAsync("nonexistent-slug");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsEntity()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var video = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(video);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.GetByIdAsync(video.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(video.Id);
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_WhenNotFound_ThrowsNotFoundException()
    {
        var repo = Resolve<IShortVideoRepository>();

        var act = () => repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_PersistsShortVideoToDatabase()
    {
        var video = ShortVideoFactory.Create();
        var (repo, db) = CreateScopedRepository<IShortVideoRepository, ContentDbContext>();

        await repo.AddAsync(video);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.ShortVideos.FirstOrDefaultAsync(v => v.Id == video.Id);

        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be(video.Title);
    }

    [Fact]
    public async Task Remove_DeletesShortVideoFromDatabase()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var video = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(video);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IShortVideoRepository, ContentDbContext>();
        var toRemove = await db.ShortVideos.FindAsync(video.Id);
        repo.Remove(toRemove!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var removed = await verifyContext.ShortVideos.FindAsync(video.Id);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task HasLikedAsync_WhenLikeExists_ReturnsTrue()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var video = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(video);
        await seedContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var like = ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, video.Id);
        seedContext.ShortVideoLikes.Add(like);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.HasLikedAsync(userId, video.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasLikedAsync_WhenNoLikeExists_ReturnsFalse()
    {
        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.HasLikedAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
    }
}
