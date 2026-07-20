using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetOwnArticleBookmarks.V1;

/// <summary>
/// Integration tests for the PublicGetOwnArticleBookmarks endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetOwnArticleBookmarksEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task GetOwnArticleBookmarks_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOwnArticleBookmarks_AsVisitor_ReturnsOk()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserBookmarkedArticleDto> body = await response.ReadAsAsync<
            PaginatedResult<UserBookmarkedArticleDto>
        >();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOwnArticleBookmarks_AsVisitor_ReturnsBookmarkedArticle()
    {
        ArticleEntity article = await SeedBookmarkedArticleAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync(BookmarksUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserBookmarkedArticleDto> body = await response.ReadAsAsync<
            PaginatedResult<UserBookmarkedArticleDto>
        >();
        body.Count.Should().Be(1);
        body.Items.Should().ContainSingle(a => a.Article.Id == article.Id);
        body.Items.Single().BookmarkedAt.Should().NotBe(default);
        body.Items.Single().Article.IsBookmarked.Should().BeTrue();
    }

    [Fact]
    public async Task GetOwnArticleBookmarks_AsVisitor_WithPagination_ReturnsOk()
    {
        ArticleEntity article = await SeedBookmarkedArticleAsync(TestUser.VisitorId);
        Client.AuthenticateAsVisitor();

        var response = await Client.GetAsync($"{BookmarksUrl}?pageIndex=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<UserBookmarkedArticleDto> body = await response.ReadAsAsync<
            PaginatedResult<UserBookmarkedArticleDto>
        >();
        body.PageIndex.Should().Be(0);
        body.PageSize.Should().Be(5);
        body.Items.Should().Contain(a => a.Article.Id == article.Id);
    }
}
