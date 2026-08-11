using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles.V1;

/// <summary>
/// Integration tests for the PublicGetPopularArticles endpoint. Proves the weighted
/// engagement ordering computed in the SQL ORDER BY, the published/category/exclude
/// filters, the limit, and the cache invalidation triggered by engagement mutations.
/// </summary>
[Collection("Database")]
public class PublicGetPopularArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<Guid> SeedCategoryAsync()
    {
        return await SeedAsync<ContentDbContext, Guid>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);

            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);

            return category.Id;
        });
    }

    /// <summary>
    /// Seeds a published article. When <paramref name="publishedAt" /> is supplied the article is
    /// published at that explicit instant, so an ordering test gets its ordering from a seeded
    /// value rather than from how fast the seeding ran.
    /// </summary>
    private async Task<ArticleEntity> SeedArticleAsync(
        Guid categoryId,
        int likes = 0,
        int comments = 0,
        int shares = 0,
        int bookmarks = 0,
        DateTimeOffset? publishedAt = null
    )
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity article = publishedAt.HasValue
                ? ArticleFactory.CreatePublishedAt(categoryId, publishedAt.Value)
                : ArticleFactory.CreatePublished(categoryId);

            for (int i = 0; i < likes; i++)
            {
                article.IncrementLikeCount();
            }

            for (int i = 0; i < comments; i++)
            {
                article.IncrementCommentCount();
            }

            for (int i = 0; i < shares; i++)
            {
                article.IncrementShareCount();
            }

            for (int i = 0; i < bookmarks; i++)
            {
                article.IncrementBookmarkCount();
            }

            ctx.Articles.Add(article);

            return article;
        });
    }

    [Fact]
    public async Task GetPopularArticles_AsAnonymous_OrdersByWeightedEngagementScore()
    {
        Guid categoryId = await SeedCategoryAsync();

        ArticleEntity high = await SeedArticleAsync(categoryId, likes: 5);
        ArticleEntity mid = await SeedArticleAsync(categoryId, comments: 3);
        ArticleEntity low = await SeedArticleAsync(categoryId, shares: 4);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Select(a => a.Id).Should().ContainInOrder(high.Id, mid.Id, low.Id);
    }

    [Fact]
    public async Task GetPopularArticles_WhenScoresAreEqual_TieBreaksByPublishedAtDescending()
    {
        Guid categoryId = await SeedCategoryAsync();

        ArticleEntity older = await SeedArticleAsync(
            categoryId,
            likes: 2,
            publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        ArticleEntity newer = await SeedArticleAsync(
            categoryId,
            likes: 2,
            publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)
        );

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");

        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Select(a => a.Id).Should().ContainInOrder(newer.Id, older.Id);
    }

    [Fact]
    public async Task GetPopularArticles_ExcludesGivenId()
    {
        Guid categoryId = await SeedCategoryAsync();

        ArticleEntity top = await SeedArticleAsync(categoryId, likes: 5);
        ArticleEntity other = await SeedArticleAsync(categoryId, bookmarks: 1);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10&excludeId={top.Id}");

        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().NotContain(a => a.Id == top.Id);
        body.Articles.Should().Contain(a => a.Id == other.Id);
    }

    [Fact]
    public async Task GetPopularArticles_FiltersByCategory()
    {
        Guid categoryA = await SeedCategoryAsync();
        Guid categoryB = await SeedCategoryAsync();

        ArticleEntity inA = await SeedArticleAsync(categoryA, likes: 3);
        await SeedArticleAsync(categoryB, likes: 9);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?categoryId={categoryA}");

        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().OnlyContain(a => a.CategoryId == categoryA);
        body.Articles.Should().Contain(a => a.Id == inA.Id);
    }

    [Fact]
    public async Task GetPopularArticles_ReturnsOnlyPublished()
    {
        Guid categoryId = await SeedCategoryAsync();

        await SeedArticleAsync(categoryId, likes: 1);
        ArticleEntity draft = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity article = ArticleFactory.Create(categoryId);

            for (int i = 0; i < 50; i++)
            {
                article.IncrementShareCount();
            }

            ctx.Articles.Add(article);
            return article;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");

        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().NotContain(a => a.Id == draft.Id);
        body.Articles.Should().OnlyContain(a => a.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetPopularArticles_RespectsLimit()
    {
        Guid categoryId = await SeedCategoryAsync();

        await SeedArticleAsync(categoryId, likes: 1);
        await SeedArticleAsync(categoryId, likes: 2);
        await SeedArticleAsync(categoryId, likes: 3);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=2");

        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetPopularArticles_WithOutOfRangeLimit_ClampsAndReturnsOk()
    {
        Guid categoryId = await SeedCategoryAsync();
        await SeedArticleAsync(categoryId, likes: 1);

        Client.ClearAuthentication();

        var oversized = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=500");
        var undersized = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=0");

        oversized.StatusCode.Should().Be(HttpStatusCode.OK);
        undersized.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetPopularArticlesResponse body = await undersized.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetPopularArticles_WithCategoryExcludeAndLimit_AppliesAllFilters()
    {
        Guid categoryA = await SeedCategoryAsync();
        Guid categoryB = await SeedCategoryAsync();

        ArticleEntity excluded = await SeedArticleAsync(categoryA, likes: 9);
        ArticleEntity topKept = await SeedArticleAsync(categoryA, likes: 5);
        await SeedArticleAsync(categoryA, likes: 1);
        await SeedArticleAsync(categoryB, likes: 8);

        Client.ClearAuthentication();

        var response = await Client.GetAsync(
            $"{Routes.Public.Articles.Popular()}?categoryId={categoryA}&excludeId={excluded.Id}&limit=1"
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicGetPopularArticlesResponse body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().ContainSingle();
        body.Articles.Should().OnlyContain(a => a.CategoryId == categoryA);
        body.Articles.Should().NotContain(a => a.Id == excluded.Id);
        body.Articles.First().Id.Should().Be(topKept.Id);
    }

    [Fact]
    public async Task GetPopularArticles_AfterEngagementChange_ReflectsNewRanking()
    {
        Guid categoryId = await SeedCategoryAsync();

        ArticleEntity quiet = await SeedArticleAsync(categoryId);
        ArticleEntity leader = await SeedArticleAsync(categoryId, bookmarks: 1);

        Client.ClearAuthentication();

        var first = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse firstBody = await first.ReadAsAsync<PublicGetPopularArticlesResponse>();
        firstBody.Articles.First().Id.Should().Be(leader.Id);

        await Client.PostAsync(Routes.Public.Articles.Shares(quiet.Id), null);

        var second = await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10");
        PublicGetPopularArticlesResponse secondBody = await second.ReadAsAsync<PublicGetPopularArticlesResponse>();
        secondBody.Articles.First().Id.Should().Be(quiet.Id);
    }
}
