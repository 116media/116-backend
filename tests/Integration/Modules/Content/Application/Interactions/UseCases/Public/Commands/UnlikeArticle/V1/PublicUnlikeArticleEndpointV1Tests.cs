using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeArticle.V1;

/// <summary>
/// Integration tests for the PublicUnlikeArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnlikeArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UnlikeArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlikeArticle_AsVisitor_NonExistentLike_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that unliking an article that was never liked returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task UnlikeArticle_WhenNotLiked_ReturnsBadRequest()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var article = ArticleFactory.CreatePublished(category.Id);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync($"{ApiRoutes.Public.Articles}/{article.Id}/likes");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
