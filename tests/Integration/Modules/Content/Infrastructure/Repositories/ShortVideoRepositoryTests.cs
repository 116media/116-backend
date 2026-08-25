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

        totalCount.Should().Be(5);
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

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_WhenAnonymous_ReturnsEmptySets()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var video = ShortVideoFactory.Create();
        seedContext.ShortVideos.Add(video);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var (liked, bookmarked) = await repo.GetLikedAndBookmarkedIdsAsync(
            currentUserId: null,
            shortVideoIds: new[] { video.Id }
        );

        liked.Should().BeEmpty();
        bookmarked.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLikedAndBookmarkedIdsAsync_ReturnsOnlyThisUsersInteractions()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var likedVideo = ShortVideoFactory.Create();
        var bookmarkedVideo = ShortVideoFactory.Create();
        var untouchedVideo = ShortVideoFactory.Create();
        seedContext.ShortVideos.AddRange(likedVideo, bookmarkedVideo, untouchedVideo);

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        seedContext.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), userId, likedVideo.Id));
        seedContext.ShortVideoBookmarks.Add(
            ShortVideoBookmarkEntity.Create(Guid.NewGuid(), userId, bookmarkedVideo.Id)
        );
        seedContext.ShortVideoLikes.Add(ShortVideoLikeEntity.Create(Guid.NewGuid(), otherUserId, untouchedVideo.Id));
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var (liked, bookmarked) = await repo.GetLikedAndBookmarkedIdsAsync(
            currentUserId: userId,
            shortVideoIds: new[] { likedVideo.Id, bookmarkedVideo.Id, untouchedVideo.Id }
        );

        liked.Should().BeEquivalentTo(new[] { likedVideo.Id });
        bookmarked.Should().BeEquivalentTo(new[] { bookmarkedVideo.Id });
    }

    [Fact]
    public async Task GetRandomizedFeedAsync_ReturnsOnlyActiveShorts()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var active = ShortVideoFactory.Create();
        var inactive = ShortVideoFactory.CreateInactive();
        seedContext.ShortVideos.AddRange(active, inactive);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var result = await repo.GetRandomizedFeedAsync(seed: 123, afterSortKey: null, limit: 50);

        result.Should().Contain(s => s.Id == active.Id);
        result.Should().NotContain(s => s.Id == inactive.Id);
    }

    [Fact]
    public async Task GetRandomizedFeedAsync_WithKeysetResume_MatchesTheContinuousOrdering()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shorts = ShortVideoFactory.CreateMany(6);
        seedContext.ShortVideos.AddRange(shorts);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();
        const long seed = 777;

        var full = await repo.GetRandomizedFeedAsync(seed, afterSortKey: null, limit: 6);
        full.Should().HaveCount(6);

        var firstPage = await repo.GetRandomizedFeedAsync(seed, afterSortKey: null, limit: 3);
        long afterKey = firstPage[^1].FeedRank ^ seed;
        var secondPage = await repo.GetRandomizedFeedAsync(seed, afterKey, limit: 3);

        firstPage.Select(s => s.Id).Should().Equal(full.Take(3).Select(s => s.Id));
        secondPage.Select(s => s.Id).Should().Equal(full.Skip(3).Take(3).Select(s => s.Id));
        firstPage.Select(s => s.Id).Should().NotIntersectWith(secondPage.Select(s => s.Id));
    }

    [Fact]
    public async Task GetRandomizedFeedAsync_IsStableForTheSameSeed()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shorts = ShortVideoFactory.CreateMany(5);
        seedContext.ShortVideos.AddRange(shorts);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        var first = await repo.GetRandomizedFeedAsync(seed: 42, afterSortKey: null, limit: 5);
        var second = await repo.GetRandomizedFeedAsync(seed: 42, afterSortKey: null, limit: 5);

        first.Select(s => s.Id).Should().Equal(second.Select(s => s.Id));
    }

    [Fact]
    public async Task GetRandomizedFeedAsync_WithDifferentSeeds_ReshufflesTheOrdering()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var shorts = ShortVideoFactory.CreateMany(20);
        seedContext.ShortVideos.AddRange(shorts);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IShortVideoRepository>();

        // Seeds differing in the sign bit swap the negative/positive halves of the key space,
        // reshuffling the order unless all ranks happen to share a sign (~2e-6 for 20 rows).
        var first = await repo.GetRandomizedFeedAsync(seed: 0, afterSortKey: null, limit: 20);
        var second = await repo.GetRandomizedFeedAsync(seed: long.MinValue, afterSortKey: null, limit: 20);

        first.Select(s => s.Id).Should().NotEqual(second.Select(s => s.Id));
    }
}
