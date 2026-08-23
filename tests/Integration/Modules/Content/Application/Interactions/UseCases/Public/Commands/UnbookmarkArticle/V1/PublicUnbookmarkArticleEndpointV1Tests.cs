using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkArticle.V1;

/// <summary>
/// Integration tests for the PublicUnbookmarkArticle endpoint.
/// </summary>
[Collection("Database")]
public class PublicUnbookmarkArticleEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<ArticleEntity> SeedArticleAsync()
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(article);
            return article;
        });
    }

    [Fact]
    public async Task UnbookmarkArticle_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Bookmarks(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnbookmarkArticle_AsVisitor_NonExistentArticle_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Bookmarks(Guid.NewGuid()));

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
        );
    }

    [Fact]
    public async Task UnbookmarkArticle_WhenBookmarked_RemovesBookmarkAndPersists()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();
        await Client.PostAsync(Routes.Public.Articles.Bookmarks(article.Id), null);

        var response = await Client.DeleteAsync(Routes.Public.Articles.Bookmarks(article.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicUnbookmarkArticleResponse>();
        body.IsSuccess.Should().BeTrue();

        await using var verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleBookmarks.AnyAsync(b => b.ArticleId == article.Id && b.UserId == TestUser.VisitorId))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task UnbookmarkArticle_WhenNotBookmarked_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.DeleteAsync(Routes.Public.Articles.Bookmarks(article.Id));

        await response.ShouldBeProblem<BadRequestException>(
            HttpStatusCode.BadRequest,
            Localized<ArticleInteractionErrorMessage>(m => m.BookmarkNotFound())
        );
    }
}
