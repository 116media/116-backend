using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Pagination;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;

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

    [Fact]
    public async Task GetArticleComments_WithSeededComment_ReturnsAuthor()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        ArticleCommentDto dto = body.Items.Single(c => c.Id == comment.Id);
        dto.Author.Should().NotBeNull();
        dto.Author!.UserName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetArticleComments_AuthorEmail_IsNotExposed()
    {
        (ArticleEntity article, ArticleCommentEntity comment) = await SeedArticleWithCommentAsync();
        Client.ClearAuthentication();

        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Single(c => c.Id == comment.Id).Author!.Email.Should().BeNull();
    }

    [Fact]
    public async Task GetArticleComments_DeletedComment_HasNoAuthor()
    {
        (ArticleEntity article, _) = await SeedArticleWithCommentAsync();

        ArticleCommentEntity deleted = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity c = ArticleCommentFactory.Create(article.Id, TestUser.VisitorId);
            c.SoftDelete();
            ctx.ArticleComments.Add(c);
            return c;
        });

        Client.ClearAuthentication();
        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        ArticleCommentDto dto = body.Items.Single(c => c.Id == deleted.Id);
        dto.Body.Should().BeNull();
        dto.Author.Should().BeNull();
    }

    [Fact]
    public async Task GetArticleComments_WithMultipleCommenters_ReturnsEachAuthor()
    {
        (ArticleEntity article, ArticleCommentEntity firstComment) = await SeedArticleWithCommentAsync();

        var secondUserId = Guid.NewGuid();
        await SeedAsync<IdentityDbContext, UserEntity>(ctx =>
        {
            UserEntity user = UserFactory.CreateWithId(secondUserId, "second-commenter@116.com");
            ctx.Users.Add(user);
            return user;
        });

        ArticleCommentEntity secondComment = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity c = ArticleCommentFactory.Create(article.Id, secondUserId);
            ctx.ArticleComments.Add(c);
            return c;
        });

        Client.ClearAuthentication();
        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Single(c => c.Id == firstComment.Id).Author.Should().NotBeNull();
        body.Items.Single(c => c.Id == secondComment.Id).Author.Should().NotBeNull();
    }

    [Fact]
    public async Task GetArticleComments_ExcludesReplies_FromTopLevelList()
    {
        (ArticleEntity article, ArticleCommentEntity topLevel) = await SeedArticleWithCommentAsync();

        ArticleCommentEntity reply = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity r = ArticleCommentEntity.CreateReply(
                Guid.NewGuid(),
                TestUser.VisitorId,
                article.Id,
                topLevel.Id,
                "a reply"
            );
            ctx.ArticleComments.Add(r);
            return r;
        });

        Client.ClearAuthentication();
        var response = await Client.GetAsync(Routes.Public.Articles.Comments(article.Id));

        PaginatedResult<ArticleCommentDto> body = await response.ReadAsAsync<PaginatedResult<ArticleCommentDto>>();
        body.Items.Should().Contain(c => c.Id == topLevel.Id);
        body.Items.Should().NotContain(c => c.Id == reply.Id);
        body.Items.Single(c => c.Id == topLevel.Id).ReplyCount.Should().Be(1);
    }
}
