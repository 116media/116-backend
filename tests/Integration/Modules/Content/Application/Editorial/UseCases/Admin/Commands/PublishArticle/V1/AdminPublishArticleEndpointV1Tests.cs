using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle.V1;

/// <summary>
/// Integration tests for the AdminPublishArticle endpoint.
/// </summary>
[Collection("Database")]
public class AdminPublishArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task PublishArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PublishArticle_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishArticle_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishArticle_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, Guid.NewGuid()),
            null
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task PublishArticle_WhenAlreadyPublished_ReturnsConflict()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreatePublished);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, article.Id),
            null
        );

        await response.ShouldBeProblem<ConflictException>(
            HttpStatusCode.Conflict,
            Localized<ArticleErrorMessage>(m => m.AlreadyPublished())
        );
        (await GetArticleAsync(article.Id)).Status.Should().Be(EnumContentStatus.Published);
    }

    [Fact]
    public async Task PublishArticle_WhenDraft_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.Create);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, article.Id),
            null
        );

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ArticleErrorMessage>(m =>
                m.InvalidStatusTransition(
                    from: nameof(EnumContentStatus.Draft),
                    to: nameof(EnumContentStatus.Published)
                )
            )
        );
        (await GetArticleAsync(article.Id)).Status.Should().Be(EnumContentStatus.Draft);
    }

    [Fact]
    public async Task PublishArticle_AsSuperAdmin_ApprovedArticle_ReturnsOk()
    {
        ArticleEntity article = await SeedArticleAsync(ArticleFactory.CreateApproved);
        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync(
            Routes.Admin.Editorial.Publish(EditorialRouteConstants.Articles, article.Id),
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminPublishArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        ArticleEntity persisted = await GetArticleAsync(article.Id);
        persisted.Status.Should().Be(EnumContentStatus.Published);
        persisted.PublishedAt.Should().NotBeNull();
    }
}
