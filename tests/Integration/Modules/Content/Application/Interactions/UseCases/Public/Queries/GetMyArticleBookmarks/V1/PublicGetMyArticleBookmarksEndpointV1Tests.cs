using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetMyArticleBookmarks.V1;

/// <summary>
/// Integration tests for the PublicGetMyArticleBookmarks endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetMyArticleBookmarksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string BookmarksUrl => $"{ApiRoutes.Public.Articles}/{InteractionsRouteConstants.Bookmarks}";

    private async Task<ArticleEntity> SeedBookmarkedArticleAsync(Guid userId)
    {
        return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(article);
            ctx.ArticleBookmarks.Add(ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, article.Id));
            return article;
        });
    }

    [Fact]
    public async Task GetMyArticleBookmarks_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyArticleBookmarks_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleSummaryDto> body = await response.ReadAsAsync<PaginatedResult<ArticleSummaryDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMyArticleBookmarks_AsVisitor_ReturnsBookmarkedArticle()
    {
        ArticleEntity article = await SeedBookmarkedArticleAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleSummaryDto> body = await response.ReadAsAsync<PaginatedResult<ArticleSummaryDto>>();
        body.Count.Should().Be(1);
        body.Items.Should().ContainSingle(a => a.Id == article.Id);
    }

    [Fact]
    public async Task GetMyArticleBookmarks_AsVisitor_WithPagination_ReturnsOk()
    {
        ArticleEntity article = await SeedBookmarkedArticleAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{BookmarksUrl}?pageIndex=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleSummaryDto> body = await response.ReadAsAsync<PaginatedResult<ArticleSummaryDto>>();
        body.PageIndex.Should().Be(0);
        body.PageSize.Should().Be(5);
        body.Items.Should().Contain(a => a.Id == article.Id);
    }
}
