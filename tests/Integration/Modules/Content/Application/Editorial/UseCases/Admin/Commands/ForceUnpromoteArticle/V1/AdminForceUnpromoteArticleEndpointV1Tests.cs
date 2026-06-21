using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.V1;

/// <summary>
/// Integration tests for the AdminForceUnpromoteArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminForceUnpromoteArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ForceUnpromoteArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/non-existent-slug/unpromote",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_WithNonExistentSlug_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/non-existent-slug/unpromote",
            new { Reason = "Content policy violation" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_WithPromotedArticle_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var promotionLevel = PromotionLevelFactory.Create();
        var article = ArticleFactory.CreatePublished(category.Id);
        article.StampPromotion(promotionLevel.Id, DateTimeOffset.UtcNow.AddDays(7));
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.PromotionLevels.Add(promotionLevel);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Slug}/unpromote",
            new { Reason = "Content policy violation" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that force-unpromoting an article that does not have an active promotion
    /// returns a 400 Bad Request response, exercising the <c>!IsPromoted</c> guard
    /// in <c>ArticleEntity.ForceUnpromote</c>.
    /// </summary>
    [Fact]
    public async Task ForceUnpromoteArticle_AsSuperAdmin_NotPromoted_ReturnsBadRequest()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.CreatePublished(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Slug}/unpromote",
            new { Reason = "Attempted unpromote on non-promoted article" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
