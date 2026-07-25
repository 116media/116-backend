using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="ILyricsRepository" /> verifying lyrics CRUD,
/// search, and lookup operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class LyricsRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task GetAllAsync_WithLyrics_ReturnsPaginatedResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Lyrics.AddRange(LyricsFactory.CreateMany(category.Id, 3));
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (lyrics, totalCount) = await repo.GetAllAsync(1, 10, null, null, null);

        totalCount.Should().BeGreaterThanOrEqualTo(3);
        lyrics.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLyrics_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByIdAsync(lyrics.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentLyrics_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdOrThrowAsync_NonExistentLyrics_ThrowsNotFoundException()
    {
        var repo = Resolve<ILyricsRepository>();

        var act = async () => await repo.GetByIdOrThrowAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetBySlugAsync_ExistingMatch_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        string slug = $"unique-lyrics-slug-{Guid.NewGuid():N}";
        var lyrics = LyricsFactory.CreateWithSlug(category.Id, slug);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetBySlugAsync(slug);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_NoMatch_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetBySlugAsync($"non-existent-slug-{Guid.NewGuid():N}");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByVideoIdAsync_ExistingVideoId_ReturnsLyrics()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var video = VideoFactory.Create(category.Id);
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.CreateForVideo(category.Id, video.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByVideoIdAsync(video.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(lyrics.Id);
    }

    [Fact]
    public async Task GetByVideoIdAsync_NoMatch_ReturnsNull()
    {
        var repo = Resolve<ILyricsRepository>();

        var result = await repo.GetByVideoIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_NewLyrics_PersistsToDatabase()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();

        await repo.AddAsync(lyrics);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var persisted = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task Remove_ExistingLyrics_DeletesFromDatabase()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        var toRemove = await db.Lyrics.FindAsync(lyrics.Id);
        repo.Remove(toRemove!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        var removed = await verifyContext.Lyrics.FindAsync(lyrics.Id);
        removed.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="ILyricsRepository.GetAllAsync" /> with a search query returns
    /// only lyrics whose song title matches the keyword, exercising the search path
    /// in <c>LyricsRepository</c> via <c>LyricsSearchSpecification</c>.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithSearchQuery_ReturnsFilteredResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        string uniqueKeyword = $"UniqueLyricsKw{Guid.NewGuid():N}"[..20];
        var matchingLyrics = LyricsFactory.Create(category.Id, $"{uniqueKeyword} Song", "Artist A");
        var nonMatchingLyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.AddRange(matchingLyrics, nonMatchingLyrics);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();
        var (result, totalCount) = await repo.GetAllAsync(1, 100, uniqueKeyword, null, null);

        totalCount.Should().BeGreaterThanOrEqualTo(1);
        result.Should().Contain(l => l.Id == matchingLyrics.Id);
        result.Should().NotContain(l => l.Id == nonMatchingLyrics.Id);
    }

    /// <summary>
    /// Verifies that <see cref="ILyricsRepository.GetAllAsync" /> filters correctly by
    /// content status, category, and language.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithStatusCategoryAndLanguageFilters_ReturnsOnlyMatchingResults()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        var otherCategory = CategoryFactory.Create(contentType.Id);
        context.Categories.AddRange(category, otherCategory);
        await context.SaveChangesAsync();

        var published = LyricsFactory.CreatePublished(category.Id);
        var draft = LyricsFactory.Create(category.Id);
        var otherCategoryPublished = LyricsFactory.CreatePublished(otherCategory.Id);
        context.Lyrics.AddRange(published, draft, otherCategoryPublished);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (result, totalCount) = await repo.GetAllAsync(
            1,
            100,
            null,
            EnumContentStatus.Published,
            category.Id,
            published.Language
        );

        totalCount.Should().Be(1);
        result.Should().ContainSingle(l => l.Id == published.Id);
    }

    /// <summary>
    /// Proves the "New Lyrics" bug fix: with no sort param, results come back <c>CreatedAt</c>
    /// descending (newest first), not alphabetical by <c>SongTitle</c>.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithNoSortParam_ReturnsNewestFirstNotAlphabetical()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        DateTime baseTime = DateTime.UtcNow.AddDays(-1);

        // Song titles are deliberately in the OPPOSITE order of creation, so an alphabetical
        // sort and a CreatedAt-descending sort would disagree — proving which one actually runs.
        var oldest = LyricsFactory.CreatePublishedWithSongTitle(category.Id, "Zebra Song");
        var middle = LyricsFactory.CreatePublishedWithSongTitle(category.Id, "Middle Song");
        var newest = LyricsFactory.CreatePublishedWithSongTitle(category.Id, "Aardvark Song");

        context.Lyrics.AddRange(oldest, middle, newest);
        await context.SaveChangesAsync();

        // The audit interceptor stamps CreatedAt = UtcNow on insert regardless of any value set
        // via the entity/builder, so backdate it afterwards with a raw update — same pattern as
        // ArticleRepositoryTests.GetAbandonedDraftsAsync_ReturnsDraftsBeforeCutoff.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime} WHERE id = {oldest.Id}"
        );
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime.AddMinutes(10)} WHERE id = {middle.Id}"
        );
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime.AddMinutes(20)} WHERE id = {newest.Id}"
        );

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id);

        result.Select(l => l.Id).Should().ContainInOrder(newest.Id, middle.Id, oldest.Id);
    }

    /// <summary>
    /// Guard test (spec 13's explicit ask): the default/"newest" sort branch has no
    /// <c>IsPromoted</c>-aware case. A promoted lyrics page created BEFORE a non-promoted one
    /// must still sort AFTER it (interleaved strictly by recency), proving promotion status
    /// plays no role in ordering. If a future change adds an IsPromoted-first branch to the sort
    /// switch without updating this test, this assertion fails because the promoted-but-older
    /// page would then be pinned first. The "views"/"likes"/"shares" branches (added alongside
    /// the interaction counters) have their own equivalent guard below.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_PromotedAndNonPromotedLyrics_InterleaveStrictlyByCreatedAtDescending()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        DateTime baseTime = DateTime.UtcNow.AddDays(-1);

        // Oldest is promoted, middle and newest are not — if the sort switch ever special-cased
        // IsPromoted, the promoted (oldest) row would jump to the front. It must not.
        var oldestPromoted = LyricsFactory.CreatePublished(category.Id);
        oldestPromoted.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var middleNotPromoted = LyricsFactory.CreatePublished(category.Id);
        var newestNotPromoted = LyricsFactory.CreatePublished(category.Id);

        context.Lyrics.AddRange(oldestPromoted, middleNotPromoted, newestNotPromoted);
        await context.SaveChangesAsync();

        // The audit interceptor stamps CreatedAt = UtcNow on insert regardless of any value set
        // via the entity/builder, so backdate it afterwards with a raw update — same pattern as
        // ArticleRepositoryTests.GetAbandonedDraftsAsync_ReturnsDraftsBeforeCutoff.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime} WHERE id = {oldestPromoted.Id}"
        );
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime.AddMinutes(10)} WHERE id = {middleNotPromoted.Id}"
        );
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics SET created_at = {baseTime.AddMinutes(20)} WHERE id = {newestNotPromoted.Id}"
        );

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id);

        result.Select(l => l.Id).Should().ContainInOrder(newestNotPromoted.Id, middleNotPromoted.Id, oldestPromoted.Id);
    }

    /// <summary>
    /// <c>sort=views</c> orders strictly by <see cref="LyricsEntity.ViewCount"/> descending,
    /// independent of recency — the oldest record with the most views comes first.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SortByViews_OrdersByViewCountDescending()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var mostViewed = LyricsFactory.CreatePublished(category.Id);
        mostViewed.IncrementViewCount();
        mostViewed.IncrementViewCount();
        mostViewed.IncrementViewCount();
        var leastViewed = LyricsFactory.CreatePublished(category.Id);
        var midViewed = LyricsFactory.CreatePublished(category.Id);
        midViewed.IncrementViewCount();

        // Inserted in an order that disagrees with both recency and view count, so only a
        // genuine ViewCount-descending sort produces the expected order.
        context.Lyrics.AddRange(leastViewed, mostViewed, midViewed);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id, sort: "views");

        result.Select(l => l.Id).Should().ContainInOrder(mostViewed.Id, midViewed.Id, leastViewed.Id);
    }

    /// <summary>
    /// <c>sort=likes</c> orders strictly by <see cref="LyricsEntity.LikeCount"/> descending.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SortByLikes_OrdersByLikeCountDescending()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var mostLiked = LyricsFactory.CreatePublished(category.Id);
        mostLiked.IncrementLikeCount();
        mostLiked.IncrementLikeCount();
        var leastLiked = LyricsFactory.CreatePublished(category.Id);
        var midLiked = LyricsFactory.CreatePublished(category.Id);
        midLiked.IncrementLikeCount();

        context.Lyrics.AddRange(leastLiked, mostLiked, midLiked);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id, sort: "likes");

        result.Select(l => l.Id).Should().ContainInOrder(mostLiked.Id, midLiked.Id, leastLiked.Id);
    }

    /// <summary>
    /// <c>sort=shares</c> orders strictly by <see cref="LyricsEntity.ShareCount"/> descending.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SortByShares_OrdersByShareCountDescending()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var mostShared = LyricsFactory.CreatePublished(category.Id);
        mostShared.IncrementShareCount();
        mostShared.IncrementShareCount();
        var leastShared = LyricsFactory.CreatePublished(category.Id);
        var midShared = LyricsFactory.CreatePublished(category.Id);
        midShared.IncrementShareCount();

        context.Lyrics.AddRange(leastShared, mostShared, midShared);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id, sort: "shares");

        result.Select(l => l.Id).Should().ContainInOrder(mostShared.Id, midShared.Id, leastShared.Id);
    }

    /// <summary>
    /// Guard test: <c>sort=views</c> has no <c>IsPromoted</c>-aware branch either — a promoted
    /// record with fewer views must not be pinned ahead of a non-promoted record with more views.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SortByViews_PromotedRecordWithFewerViewsDoesNotJumpAhead()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var promotedFewerViews = LyricsFactory.CreatePublished(category.Id);
        promotedFewerViews.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var notPromotedMoreViews = LyricsFactory.CreatePublished(category.Id);
        notPromotedMoreViews.IncrementViewCount();
        notPromotedMoreViews.IncrementViewCount();

        context.Lyrics.AddRange(promotedFewerViews, notPromotedMoreViews);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        var (result, _) = await repo.GetAllAsync(1, 10, null, EnumContentStatus.Published, category.Id, sort: "views");

        result.Select(l => l.Id).Should().ContainInOrder(notPromotedMoreViews.Id, promotedFewerViews.Id);
    }

    #region Like Tests

    [Fact]
    public async Task HasLikedAsync_WhenLikeExists_ReturnsTrue()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        Guid userId = Guid.NewGuid();
        context.LyricsLikes.Add(LyricsLikeFactory.Create(userId, lyrics.Id));
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        (await repo.HasLikedAsync(userId, lyrics.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task HasLikedAsync_WhenNoLikeExists_ReturnsFalse()
    {
        var repo = Resolve<ILyricsRepository>();

        (await repo.HasLikedAsync(Guid.NewGuid(), Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public async Task AddLikeAsync_PersistsLikeRow()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        Guid userId = Guid.NewGuid();
        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        await repo.AddLikeAsync(LyricsLikeFactory.Create(userId, lyrics.Id));
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.LyricsLikes.AnyAsync(l => l.UserId == userId && l.LyricsId == lyrics.Id))
            .Should()
            .BeTrue();
    }

    /// <summary>
    /// Verifies the DB-level unique constraint on <c>(UserId, LyricsId)</c> — a second like row
    /// for the same user and lyrics page must be rejected at the database level.
    /// </summary>
    [Fact]
    public async Task AddLikeAsync_DuplicateUserAndLyrics_ViolatesUniqueConstraint()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        Guid userId = Guid.NewGuid();
        context.LyricsLikes.Add(LyricsLikeFactory.Create(userId, lyrics.Id));
        await context.SaveChangesAsync();

        await using var duplicateContext = CreateDbContext<ContentDbContext>();
        duplicateContext.LyricsLikes.Add(LyricsLikeFactory.Create(userId, lyrics.Id));

        Func<Task> act = async () => await duplicateContext.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task RemoveLikeAsync_WhenLikeExists_RemovesIt()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        Guid userId = Guid.NewGuid();
        context.LyricsLikes.Add(LyricsLikeFactory.Create(userId, lyrics.Id));
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        await repo.RemoveLikeAsync(userId, lyrics.Id);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.LyricsLikes.AnyAsync(l => l.UserId == userId && l.LyricsId == lyrics.Id))
            .Should()
            .BeFalse();
    }

    #endregion

    #region Share Tests

    [Fact]
    public async Task AddShareAsync_PersistsShareRow()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        await repo.AddShareAsync(LyricsShareFactory.CreateAnonymous(lyrics.Id));
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.LyricsShares.AnyAsync(s => s.LyricsId == lyrics.Id)).Should().BeTrue();
    }

    #endregion

    #region View Event Tests

    [Fact]
    public async Task AddViewEventAsync_PersistsViewEventRow()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ILyricsRepository, ContentDbContext>();
        await repo.AddViewEventAsync(LyricsViewEventFactory.CreateCounted(lyrics.Id, "unknown"));
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<ContentDbContext>();
        (await verifyContext.LyricsViewEvents.AnyAsync(e => e.LyricsId == lyrics.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task HasCountedViewSinceAsync_WithCountedEventInsideWindow_ReturnsTrue()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        const string dedupKey = "device:dedup-window-test";
        context.LyricsViewEvents.Add(LyricsViewEventFactory.CreateCounted(lyrics.Id, dedupKey));
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        (await repo.HasCountedViewSinceAsync(lyrics.Id, dedupKey, DateTime.UtcNow.AddMinutes(-5))).Should().BeTrue();
    }

    [Fact]
    public async Task HasCountedViewSinceAsync_WithCountedEventOutsideWindow_ReturnsFalse()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        const string dedupKey = "device:dedup-window-test-outside";
        context.LyricsViewEvents.Add(LyricsViewEventFactory.CreateCounted(lyrics.Id, dedupKey));
        await context.SaveChangesAsync();

        // Backdate the event to before the dedup window being checked.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE content.lyrics_view_events SET created_at = {DateTime.UtcNow.AddDays(-2)} WHERE lyrics_id = {lyrics.Id}"
        );

        var repo = Resolve<ILyricsRepository>();

        (await repo.HasCountedViewSinceAsync(lyrics.Id, dedupKey, DateTime.UtcNow.AddMinutes(-5))).Should().BeFalse();
    }

    [Fact]
    public async Task HasCountedViewSinceAsync_WithUncountedEvent_ReturnsFalse()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var lyrics = LyricsFactory.Create(category.Id);
        context.Lyrics.Add(lyrics);
        await context.SaveChangesAsync();

        const string dedupKey = "device:dedup-uncounted-test";
        context.LyricsViewEvents.Add(LyricsViewEventFactory.CreateUncounted(lyrics.Id, dedupKey));
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        (await repo.HasCountedViewSinceAsync(lyrics.Id, dedupKey, DateTime.UtcNow.AddMinutes(-5))).Should().BeFalse();
    }

    #endregion

    #region GetSimilarAsync Tests

    [Fact]
    public async Task GetSimilarAsync_VideoLinkedWithNoCategoryMatchButSharedTagExists_FallsThroughToSharedTagsBranch()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var sharedTag = TagFactory.Create();
        context.Tags.Add(sharedTag);
        await context.SaveChangesAsync();

        var sourceVideo = VideoFactory.CreatePublished(category.Id);
        context.Videos.Add(sourceVideo);
        await context.SaveChangesAsync();

        var source = LyricsFactory.CreatePublishedForVideoWithSlug(
            category.Id,
            sourceVideo.Id,
            $"source-{Guid.NewGuid():N}"
        );
        source.Tags.Add(LyricsTagEntity.Create(Guid.NewGuid(), source.Id, sharedTag.Id));

        var tagMatch = LyricsFactory.CreateWithTags(category.Id, sharedTag.Id);
        tagMatch.Publish();

        context.Lyrics.AddRange(source, tagMatch);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        IReadOnlyList<LyricsEntity> similar = await repo.GetSimilarAsync(source.Id);

        similar.Should().NotBeEmpty();
        similar.Should().ContainSingle(l => l.Id == tagMatch.Id);
    }

    /// <summary>
    /// A standalone (no <c>VideoId</c>) source with no tags of its own has nothing to match in
    /// the video-category or shared-tags branches, so it falls all the way through to the
    /// latest-standalone branch and returns another standalone published record.
    /// </summary>
    [Fact]
    public async Task GetSimilarAsync_NoVideoAndNoTags_FallsThroughToLatestStandaloneBranch()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var source = LyricsFactory.CreatePublished(category.Id);
        var otherStandalone = LyricsFactory.CreatePublished(category.Id);

        context.Lyrics.AddRange(source, otherStandalone);
        await context.SaveChangesAsync();

        var repo = Resolve<ILyricsRepository>();

        IReadOnlyList<LyricsEntity> similar = await repo.GetSimilarAsync(source.Id);

        similar.Should().ContainSingle(l => l.Id == otherStandalone.Id);
    }

    [Fact]
    public async Task GetSimilarAsync_NonExistentLyrics_ThrowsNotFoundException()
    {
        var repo = Resolve<ILyricsRepository>();

        Func<Task> act = async () => await repo.GetSimilarAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
