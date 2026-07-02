using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="PopularVideosQueryBuilder"/>.
/// Uses an InMemory <see cref="ContentDbContext"/> seeded with a real content type
/// and two categories, so the weighted-score ordering and the
/// published/category/exclude/limit filters run through the full LINQ pipeline
/// (including the <c>Category</c> include) without a real database.
/// </summary>
public class PopularVideosQueryBuilderTests : IAsyncLifetime
{
    private readonly ContentDbContext _context;

    private Guid _primaryCategoryId;
    private Guid _secondaryCategoryId;

    public PopularVideosQueryBuilderTests()
    {
        DbContextOptions<ContentDbContext> options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        _context.ContentTypes.Add(contentType);

        CategoryEntity primaryCategory = CategoryFactory.Create(contentType.Id);
        CategoryEntity secondaryCategory = CategoryFactory.Create(contentType.Id);
        _context.Categories.AddRange(primaryCategory, secondaryCategory);

        await _context.SaveChangesAsync();

        _primaryCategoryId = primaryCategory.Id;
        _secondaryCategoryId = secondaryCategory.Id;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private async Task<VideoEntity> SeedPublishedAsync(
        Guid categoryId,
        decimal ratingAverage = 0m,
        int ratingCount = 0,
        int shares = 0
    )
    {
        VideoEntity video = VideoFactory.CreatePublished(categoryId);

        if (ratingCount > 0)
        {
            video.UpdateRating(average: ratingAverage, count: ratingCount);
        }

        for (int index = 0; index < shares; index++)
        {
            video.IncrementShareCount();
        }

        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
        return video;
    }

    private async Task<VideoEntity> SeedDraftAsync(Guid categoryId)
    {
        VideoEntity video = VideoFactory.Create(categoryId);
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
        return video;
    }

    #region Scoring / ordering

    [Fact]
    public async Task Build_ShouldOrderByWeightedEngagementScoreDescending()
    {
        // score = rating(3) * (average * count) + share(5) * shareCount
        VideoEntity highest = await SeedPublishedAsync(_primaryCategoryId, ratingAverage: 5m, ratingCount: 10); // 150
        VideoEntity middle = await SeedPublishedAsync(_primaryCategoryId, shares: 20); // 100
        VideoEntity lowest = await SeedPublishedAsync(_primaryCategoryId, ratingAverage: 4m, ratingCount: 4); // 48

        List<VideoEntity> result = await new PopularVideosQueryBuilder().Build(_context).ToListAsync();

        result.Select(video => video.Id).Should().ContainInOrder(highest.Id, middle.Id, lowest.Id);
    }

    [Fact]
    public async Task Build_ShouldRankBroadlyRatedVideoAboveLuckyPerfectAverage()
    {
        // rating volume is weighted by quality, so many good ratings outrank a few perfect ones
        VideoEntity broadlyRated = await SeedPublishedAsync(_primaryCategoryId, ratingAverage: 4m, ratingCount: 100); // 1200
        VideoEntity luckyAverage = await SeedPublishedAsync(_primaryCategoryId, ratingAverage: 5m, ratingCount: 2); // 30

        List<VideoEntity> result = await new PopularVideosQueryBuilder().Build(_context).ToListAsync();

        result.Select(video => video.Id).Should().ContainInOrder(broadlyRated.Id, luckyAverage.Id);
    }

    #endregion

    #region Published-only filter

    [Fact]
    public async Task Build_ShouldReturnOnlyPublishedVideos()
    {
        VideoEntity published = await SeedPublishedAsync(_primaryCategoryId, shares: 1);
        VideoEntity draft = await SeedDraftAsync(_primaryCategoryId);

        List<VideoEntity> result = await new PopularVideosQueryBuilder().Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(video => video.Id == published.Id);
        result.Should().NotContain(video => video.Id == draft.Id);
    }

    #endregion

    #region WithCategory

    [Fact]
    public async Task Build_WithCategory_ShouldReturnOnlyVideosInThatCategory()
    {
        VideoEntity inPrimary = await SeedPublishedAsync(_primaryCategoryId, shares: 2);
        VideoEntity inSecondary = await SeedPublishedAsync(_secondaryCategoryId, shares: 9);

        List<VideoEntity> result = await new PopularVideosQueryBuilder()
            .WithCategory(_primaryCategoryId)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(video => video.Id == inPrimary.Id);
        result.Should().NotContain(video => video.Id == inSecondary.Id);
    }

    [Fact]
    public async Task Build_WithNullCategory_ShouldReturnVideosFromAllCategories()
    {
        await SeedPublishedAsync(_primaryCategoryId, shares: 1);
        await SeedPublishedAsync(_secondaryCategoryId, shares: 1);

        List<VideoEntity> result = await new PopularVideosQueryBuilder()
            .WithCategory(null)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region WithExcludeId

    [Fact]
    public async Task Build_WithExcludeId_ShouldOmitThatVideo()
    {
        VideoEntity excluded = await SeedPublishedAsync(_primaryCategoryId, shares: 9);
        VideoEntity kept = await SeedPublishedAsync(_primaryCategoryId, shares: 1);

        List<VideoEntity> result = await new PopularVideosQueryBuilder()
            .WithExcludeId(excluded.Id)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(video => video.Id == kept.Id);
        result.Should().NotContain(video => video.Id == excluded.Id);
    }

    [Fact]
    public async Task Build_WithNullExcludeId_ShouldKeepAllVideos()
    {
        await SeedPublishedAsync(_primaryCategoryId, shares: 1);
        await SeedPublishedAsync(_primaryCategoryId, shares: 2);

        List<VideoEntity> result = await new PopularVideosQueryBuilder()
            .WithExcludeId(null)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region WithLimit

    [Fact]
    public async Task Build_WithLimit_ShouldReturnOnlyThatManyVideos()
    {
        for (int engagement = 1; engagement <= 5; engagement++)
        {
            await SeedPublishedAsync(_primaryCategoryId, shares: engagement);
        }

        List<VideoEntity> result = await new PopularVideosQueryBuilder().WithLimit(3).Build(_context).ToListAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Build_WithNullLimit_ShouldReturnAllVideos()
    {
        for (int engagement = 1; engagement <= 4; engagement++)
        {
            await SeedPublishedAsync(_primaryCategoryId, shares: engagement);
        }

        List<VideoEntity> result = await new PopularVideosQueryBuilder().WithLimit(null).Build(_context).ToListAsync();

        result.Should().HaveCount(4);
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Build_WithCategoryExcludeAndLimit_ShouldApplyAll()
    {
        VideoEntity top = await SeedPublishedAsync(_primaryCategoryId, shares: 9);
        await SeedPublishedAsync(_primaryCategoryId, shares: 5);
        await SeedPublishedAsync(_primaryCategoryId, shares: 1);
        await SeedPublishedAsync(_secondaryCategoryId, shares: 8);

        List<VideoEntity> result = await new PopularVideosQueryBuilder()
            .WithCategory(_primaryCategoryId)
            .WithExcludeId(top.Id)
            .WithLimit(1)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().OnlyContain(video => video.CategoryId == _primaryCategoryId);
        result.Should().NotContain(video => video.Id == top.Id);
    }

    #endregion
}
