using _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug.V1;

/// <summary>
/// Integration tests for the PublicGetArticleBySlug endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticleBySlugEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    [Fact]
    public async Task GetArticleBySlug_WithPublishedArticle_ReturnsArticle()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
        body.Article.Id.Should().Be(article.Id);
        body.Article.Slug.Should().Be(article.Slug);
        body.Article.Title.Should().Be(article.Title);
    }

    /// <summary>
    /// Verifies that a non-published (draft) article is excluded from public reads
    /// and the endpoint reports it as not found.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WithDraftArticle_ReturnsNotFound()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.Create(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetArticleBySlug_WithNonExistent_ReturnsNotFound()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/non-existent-slug");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that an authenticated user who has liked and bookmarked the article
    /// receives both interaction flags set to true.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WhenUserLikedAndBookmarked_ReturnsTrueFlags()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid userId = Guid.NewGuid();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), userId, entity.Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(userId, "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
        body.Article.IsLiked.Should().BeTrue();
        body.Article.IsBookmarked.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an anonymous request always receives false interaction flags,
    /// even when other users have liked the article.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WhenAnonymous_ReturnsFalseFlags()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), entity.Id));
            return entity;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
        body.Article.IsLiked.Should().BeFalse();
        body.Article.IsBookmarked.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that one user's like and bookmark never surface on another user's
    /// response — the flags are strictly per-caller.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WhenDifferentUser_DoesNotLeakAnotherUsersState()
    {
        Guid categoryId = await SeedCategoryAsync();
        Guid likingUserId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            ctx.ArticleLikes.Add(ArticleLikeEntity.Create(Guid.NewGuid(), likingUserId, entity.Id));
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), likingUserId, entity.Id));
            return entity;
        });

        Client.AuthenticateAs(otherUserId, "Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
        body.Article.IsLiked.Should().BeFalse();
        body.Article.IsBookmarked.Should().BeFalse();
    }

    /// <summary>
    /// A correctly signed token whose subject identifier is not a UUID is rejected rather than
    /// treated as an anonymous read. This endpoint reads its caller optionally, so a claim set
    /// the identity module cannot parse must surface as an authentication failure instead of
    /// silently degrading to the anonymous projection.
    /// </summary>
    [Fact]
    public async Task GetArticleBySlug_WithAnUnparsableSubjectClaim_ReturnsUnauthorized()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity entity = ArticleFactory.CreatePublished(categoryId);
            ctx.Articles.Add(entity);
            return entity;
        });

        Client.AuthenticateWithMalformedUserId("Visitor");

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

        await response.ShouldBeProblem(HttpStatusCode.Unauthorized);
    }
}
