using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.BookmarkArticle.V1;

/// <summary>
/// Integration tests for the PublicBookmarkArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicBookmarkArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task BookmarkArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BookmarkArticle_AsVisitor_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{Guid.NewGuid()}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BookmarkArticle_AsVisitor_WithValidArticle_ReturnsOk()
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

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{article.Id}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BookmarkArticle_AsVisitor_AlreadyBookmarked_ReturnsConflict()
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

        await Client.PostAsync($"{ApiRoutes.Public.Articles}/{article.Id}/bookmarks", null);

        var response = await Client.PostAsync($"{ApiRoutes.Public.Articles}/{article.Id}/bookmarks", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
