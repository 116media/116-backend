using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Builds the slug-based unpromote URL from production route constants.
    /// The endpoint matches on slug rather than id, so the typed Guid-based route
    /// helper cannot be used here.
    /// </summary>
    private static string UnpromoteUrl(string slug) =>
        $"{ApiRoutes.Admin.Articles}/{slug}/{EditorialRouteConstants.Unpromote}";

    [Fact]
    public async Task ForceUnpromoteArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), new { Reason = "test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_WithNonExistentSlug_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteArticleRequest request = new AdminForceUnpromoteArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl("non-existent-slug"), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that force-unpromoting a promoted article succeeds, returns the article id and
    /// an unpromoted timestamp, and clears the promotion on the persisted article.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_WithPromotedArticle_ReturnsOk()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            PromotionLevelEntity promotionLevel = PromotionLevelFactory.Create();
            ArticleEntity a = ArticleFactory.CreatePublished(category.Id);
            a.StampPromotion(promotionLevel.Id, DateTimeOffset.UtcNow.AddDays(7));
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.PromotionLevels.Add(promotionLevel);
            ctx.Articles.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteArticleRequest request = new AdminForceUnpromoteArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl(article.Slug), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminForceUnpromoteArticleResponse>();
        body.ArticleId.Should().Be(article.Id);
        body.UnpromotedAt.Should().NotBe(default);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? persisted = await ctx.Articles.FindAsync(article.Id);
        persisted.Should().NotBeNull();
        persisted!.IsPromoted.Should().BeFalse();
        persisted.PromotedUntil.Should().BeNull();
        persisted.UnpromotedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that force-unpromoting an article that does not have an active promotion
    /// returns a 400 Bad Request problem, exercising the <c>!IsPromoted</c> guard
    /// in <c>ArticleEntity.ForceUnpromote</c>.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_NotPromoted_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity a = ArticleFactory.CreatePublished(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(a);
            return a;
        });

        Client.AuthenticateAsSuperAdmin();
        AdminForceUnpromoteArticleRequest request = new AdminForceUnpromoteArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(UnpromoteUrl(article.Slug), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
