using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;

/// <summary>
/// Integration tests for the PublicAddArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicAddArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task AddArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new { Body = "This is a valid test comment body." };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Body = "This is a valid test comment body." };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_WithEmptyBody_ReturnsValidationError()
    {
        Client.AuthenticateAsVisitor();
        var request = new { Body = "" };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/comments", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddArticleComment_AsVisitor_WithValidData_ReturnsCreated()
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
        var request = new { Body = "This is a valid test comment body." };

        var response = await Client.PostAsJsonAsync($"{ApiRoutes.Public.Articles}/{article.Id}/comments", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }
}
