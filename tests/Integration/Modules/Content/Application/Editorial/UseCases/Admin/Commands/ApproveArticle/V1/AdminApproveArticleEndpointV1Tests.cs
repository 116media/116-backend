using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle.V1;

/// <summary>
/// Integration tests for the AdminApproveArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminApproveArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ApproveArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApproveArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{nonExistentId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that approving an article that is already in Approved status
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task ApproveArticle_WhenAlreadyApproved_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.CreateApproved(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{article.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that approving a PendingReview article succeeds and returns a 200 OK response,
    /// exercising the happy path of <c>ArticleEntity.Approve</c>.
    /// </summary>
    [Fact]
    public async Task ApproveArticle_AsSuperAdmin_PendingReviewArticle_ReturnsOk()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.CreatePendingReview(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Articles}/{article.Id}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
