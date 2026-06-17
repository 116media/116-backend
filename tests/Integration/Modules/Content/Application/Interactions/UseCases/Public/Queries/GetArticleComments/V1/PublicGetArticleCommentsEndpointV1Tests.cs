using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments.V1;

/// <summary>
/// Integration tests for the PublicGetArticleComments endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetArticleCommentsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(ArticleEntity Article, ArticleCommentEntity Comment)> SeedArticleWithCommentAsync()
    {
        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(created);
            return created;
        });

        ArticleCommentEntity comment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity created = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ctx.ArticleComments.Add(created);
            return created;
        });

        return (article, comment);
    }

    [Fact]
    public async Task GetArticleComments_AsAnonymous_ReturnsOk()
    {
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Articles.Comments(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Should().BeEmpty();
        body.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetArticleComments_WithSeededComment_ReturnsComment()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Count.Should().Be(1);
        body.Items.Should().ContainSingle(c => c.Id == comment.Id && c.Body == comment.Body);
    }

    [Fact]
    public async Task GetArticleComments_WithPagination_ReturnsOk()
    {
        (ArticleEntity article, _) = await SeedArticleWithCommentAsync();
        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{Routes.Public.Articles.Comments(article.Id)}?pageIndex=0&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.PageIndex.Should().Be(0);
        body.PageSize.Should().Be(5);
        body.Count.Should().Be(1);
    }
}
