using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;

/// <summary>
/// Integration tests for the PublicEditArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicEditArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task EditArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Body = "Updated comment body." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditArticleComment_AsVisitor_NonExistentComment_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Body = "Updated comment body." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that editing a comment that does not exist returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task EditComment_WhenNotExists_ReturnsNotFound()
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
        var request = new { Body = "Updated comment body." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{article.Id}/comments/{Guid.NewGuid()}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that editing a comment owned by another user returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task EditComment_WhenNotOwner_ReturnsBadRequest()
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

        var comment = ArticleCommentFactory.Create(article.Id, User.SuperAdminId);
        context.ArticleComments.Add(comment);
        await context.SaveChangesAsync();

        Client.AuthenticateAsVisitor();
        var request = new { Body = "Trying to edit someone else's comment." };

        var response = await Client.PutAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{article.Id}/comments/{comment.Id}",
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
