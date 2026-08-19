using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle.V1;

/// <summary>
/// Integration tests for the AdminRejectArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminRejectArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArticleEntity> SeedArticleAsync(Func<Guid, ArticleEntity> create)
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity article = create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(article);
            return article;
        });
    }

    private async Task<ArticleEntity> GetArticleAsync(Guid id)
    {
        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        ArticleEntity? article = await ctx.Articles.FindAsync(id);
        return article!;
    }

    [Fact]
    public async Task RejectArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RejectArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, Guid.NewGuid()),
            new { Reason = "test" }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        AdminRejectArticleRequest request = new AdminRejectArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task RejectArticle_WhenAlreadyRejected_ReturnsConflict()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreateRejected);
        Client.AuthenticateAsSuperAdmin();
        AdminRejectArticleRequest request = new AdminRejectArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, article.Id),
            request
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArticleErrorMessage>(m => m.AlreadyRejected())
        );
        (await GetArticleAsync(article.Id)).Status.Should().Be(EnumContentStatus.Rejected);
    }

    [Fact]
    public async Task RejectArticle_AsSuperAdmin_PendingReviewArticle_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreatePendingReview);
        Client.AuthenticateAsSuperAdmin();
        AdminRejectArticleRequest request = new AdminRejectArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, article.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRejectArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        ArticleEntity persisted = await GetArticleAsync(article.Id);
        persisted.Status.Should().Be(EnumContentStatus.Rejected);
        persisted.RejectionReason.Should().Be(request.Reason);
    }

    [Fact]
    public async Task RejectArticle_AsSuperAdmin_PublishedArticle_UnpublishesIt()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreatePublished);
        Client.AuthenticateAsSuperAdmin();
        AdminRejectArticleRequest request = new AdminRejectArticleRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Reject(EditorialRouteConstants.Articles, article.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminRejectArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        ArticleEntity persisted = await GetArticleAsync(article.Id);
        persisted.Status.Should().Be(EnumContentStatus.Rejected);
        persisted.RejectionReason.Should().Be(request.Reason);
    }
}
