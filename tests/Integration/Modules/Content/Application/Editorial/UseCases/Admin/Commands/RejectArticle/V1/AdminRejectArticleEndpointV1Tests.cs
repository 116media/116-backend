using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;

/// <summary>
/// Integration tests for the AdminRejectArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{nonExistentId}/reject",
            new { Reason = "test" }
        );

        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity);
    }

    /// <summary>
    /// Verifies that rejecting an article that is already in Rejected status
    /// returns a 409 Conflict response.
    /// </summary>
    [Fact]
    public async Task RejectArticle_WhenAlreadyRejected_ReturnsConflict()
    {
        await using var seedContext = CreateDbContext<ContentDbContext>();
        var contentType = ContentTypeFactory.Create();
        var category = CategoryFactory.Create(contentType.Id);
        var article = ArticleFactory.CreateRejected(category.Id);
        seedContext.ContentTypes.Add(contentType);
        seedContext.Categories.Add(category);
        seedContext.Articles.Add(article);
        await seedContext.SaveChangesAsync();

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Id}/reject",
            new { Reason = "duplicate rejection attempt" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifies that rejecting a PendingReview article succeeds and returns a 200 OK response,
    /// exercising the happy path of <c>ArticleEntity.Reject</c>.
    /// </summary>
    [Fact]
    public async Task RejectArticle_AsSuperAdmin_PendingReviewArticle_ReturnsOk()
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

        var response = await Client.PatchAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}/{article.Id}/reject",
            new { Reason = "Content quality insufficient" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
