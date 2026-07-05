using _116.Content.Application.Editorial.Builders;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Builders;

/// <summary>
/// Unit tests for <see cref="PopularArticlesQueryBuilder"/>.
/// Uses an InMemory <see cref="ContentDbContext"/> seeded with a real content type
/// and two categories, so the weighted-score ordering and the
/// published/category/exclude/limit filters run through the full LINQ pipeline
/// (including the <c>Category</c> include) without a real database.
/// </summary>
public class PopularArticlesQueryBuilderTests : IAsyncLifetime
{
    private readonly ContentDbContext _context;

    private Guid _primaryCategoryId;
    private Guid _secondaryCategoryId;

    public PopularArticlesQueryBuilderTests()
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

    private async Task<ArticleEntity> SeedPublishedAsync(
        Guid categoryId,
        int likes = 0,
        int comments = 0,
        int shares = 0,
        int bookmarks = 0
    )
    {
        ArticleEntity article = ArticleFactory.CreatePublished(categoryId);

        for (int index = 0; index < likes; index++)
        {
            article.IncrementLikeCount();
        }

        for (int index = 0; index < comments; index++)
        {
            article.IncrementCommentCount();
        }

        for (int index = 0; index < shares; index++)
        {
            article.IncrementShareCount();
        }

        for (int index = 0; index < bookmarks; index++)
        {
            article.IncrementBookmarkCount();
        }

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    private async Task<ArticleEntity> SeedDraftAsync(Guid categoryId)
    {
        ArticleEntity article = ArticleFactory.Create(categoryId);
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    #region Scoring / ordering

    [Fact]
    public async Task Build_ShouldOrderByWeightedEngagementScoreDescending()
    {
        // weights like=4, comment=3, share=2, bookmark=1
        ArticleEntity highest = await SeedPublishedAsync(_primaryCategoryId, likes: 5); // 20
        ArticleEntity middle = await SeedPublishedAsync(_primaryCategoryId, comments: 3); // 9
        ArticleEntity lowest = await SeedPublishedAsync(_primaryCategoryId, shares: 4); // 8

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder().Build(_context).ToListAsync();

        result.Select(article => article.Id).Should().ContainInOrder(highest.Id, middle.Id, lowest.Id);
    }

    [Fact]
    public async Task Build_ShouldRankHigherLikeCountAboveLowerBookmarkCount()
    {
        ArticleEntity oneLike = await SeedPublishedAsync(_primaryCategoryId, likes: 1); // 4
        ArticleEntity threeBookmarks = await SeedPublishedAsync(_primaryCategoryId, bookmarks: 3); // 3

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder().Build(_context).ToListAsync();

        result.Select(article => article.Id).Should().ContainInOrder(oneLike.Id, threeBookmarks.Id);
    }

    #endregion

    #region Published-only filter

    [Fact]
    public async Task Build_ShouldReturnOnlyPublishedArticles()
    {
        ArticleEntity published = await SeedPublishedAsync(_primaryCategoryId, likes: 1);
        ArticleEntity draft = await SeedDraftAsync(_primaryCategoryId);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder().Build(_context).ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(article => article.Id == published.Id);
        result.Should().NotContain(article => article.Id == draft.Id);
    }

    #endregion

    #region WithCategory

    [Fact]
    public async Task Build_WithCategory_ShouldReturnOnlyArticlesInThatCategory()
    {
        ArticleEntity inPrimary = await SeedPublishedAsync(_primaryCategoryId, likes: 2);
        ArticleEntity inSecondary = await SeedPublishedAsync(_secondaryCategoryId, likes: 9);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithCategory(_primaryCategoryId)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(article => article.Id == inPrimary.Id);
        result.Should().NotContain(article => article.Id == inSecondary.Id);
    }

    [Fact]
    public async Task Build_WithNullCategory_ShouldReturnArticlesFromAllCategories()
    {
        await SeedPublishedAsync(_primaryCategoryId, likes: 1);
        await SeedPublishedAsync(_secondaryCategoryId, likes: 1);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithCategory(null)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region WithExcludeId

    [Fact]
    public async Task Build_WithExcludeId_ShouldOmitThatArticle()
    {
        ArticleEntity excluded = await SeedPublishedAsync(_primaryCategoryId, likes: 9);
        ArticleEntity kept = await SeedPublishedAsync(_primaryCategoryId, likes: 1);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithExcludeId(excluded.Id)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().Contain(article => article.Id == kept.Id);
        result.Should().NotContain(article => article.Id == excluded.Id);
    }

    [Fact]
    public async Task Build_WithNullExcludeId_ShouldKeepAllArticles()
    {
        await SeedPublishedAsync(_primaryCategoryId, likes: 1);
        await SeedPublishedAsync(_primaryCategoryId, likes: 2);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithExcludeId(null)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(2);
    }

    #endregion

    #region WithLimit

    [Fact]
    public async Task Build_WithLimit_ShouldReturnOnlyThatManyArticles()
    {
        for (int engagement = 1; engagement <= 5; engagement++)
        {
            await SeedPublishedAsync(_primaryCategoryId, likes: engagement);
        }

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder().WithLimit(3).Build(_context).ToListAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Build_WithNullLimit_ShouldReturnAllArticles()
    {
        for (int engagement = 1; engagement <= 4; engagement++)
        {
            await SeedPublishedAsync(_primaryCategoryId, likes: engagement);
        }

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithLimit(null)
            .Build(_context)
            .ToListAsync();

        result.Should().HaveCount(4);
    }

    #endregion

    #region Chaining

    [Fact]
    public async Task Build_WithCategoryExcludeAndLimit_ShouldApplyAll()
    {
        ArticleEntity top = await SeedPublishedAsync(_primaryCategoryId, likes: 9);
        await SeedPublishedAsync(_primaryCategoryId, likes: 5);
        await SeedPublishedAsync(_primaryCategoryId, likes: 1);
        await SeedPublishedAsync(_secondaryCategoryId, likes: 8);

        List<ArticleEntity> result = await new PopularArticlesQueryBuilder()
            .WithCategory(_primaryCategoryId)
            .WithExcludeId(top.Id)
            .WithLimit(1)
            .Build(_context)
            .ToListAsync();

        result.Should().ContainSingle();
        result.Should().OnlyContain(article => article.CategoryId == _primaryCategoryId);
        result.Should().NotContain(article => article.Id == top.Id);
    }

    #endregion
}
