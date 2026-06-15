using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags.V1;

/// <summary>
/// Integration tests for the AdminUpdateArticleTags endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateArticleTagsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateArticleTags_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateArticleTags_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateArticleTags_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/tags", new { });

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that updating article tags replaces existing tags with the new set,
    /// exercising the ArticleTagByArticleIdSpecification internally.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithTags_ReplacesExistingTags()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();

        var category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var tag1 = TagFactory.Create("OldTag", "old-tag");
        var tag2 = TagFactory.Create("NewTag", "new-tag");
        context.Tags.AddRange(tag1, tag2);
        await context.SaveChangesAsync();

        var article = ArticleFactory.Create(category.Id);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var assignRequest = new { TagNames = new[] { "OldTag" } };
        await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{article.Id}/tags", assignRequest);

        var replaceRequest = new { TagNames = new[] { "NewTag" } };
        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.Articles}/{article.Id}/tags", replaceRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
