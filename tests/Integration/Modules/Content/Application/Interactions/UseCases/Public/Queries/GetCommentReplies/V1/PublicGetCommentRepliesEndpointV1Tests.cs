using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies.V1;

/// <summary>
/// Integration tests for the PublicGetCommentReplies endpoint.
/// </summary>
[Collection("Database")]
public class PublicGetCommentRepliesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private async Task<(ArticleEntity Article, ArticleCommentEntity Parent)> SeedArticleWithCommentAsync()
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

        ArticleCommentEntity parent = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity created = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            ctx.ArticleComments.Add(created);
            return created;
        });

        return (article, parent);
    }

    [Fact]
    public async Task GetReplies_ReturnsPagedRepliesWithAuthors()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedArticleWithCommentAsync();

        ArticleCommentEntity reply = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity r = ArticleCommentEntity.CreateReply(
                Guid.NewGuid(),
                TestUser.VisitorId,
                article.Id,
                parent.Id,
                "a reply body"
            );
            ctx.ArticleComments.Add(r);
            return r;
        });

        Client.ClearAuthentication();
        var response = await Client.GetAsync(Routes.Public.Articles.CommentRepliesList(parent.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        ArticleCommentDto dto = body.Items.Single(c => c.Id == reply.Id);
        dto.ParentCommentId.Should().Be(parent.Id);
        dto.Author.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReplies_WhenNone_ReturnsEmptyPage()
    {
        (_, ArticleCommentEntity parent) = await SeedArticleWithCommentAsync();

        Client.ClearAuthentication();
        var response = await Client.GetAsync(Routes.Public.Articles.CommentRepliesList(parent.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Should().BeEmpty();
    }
}
