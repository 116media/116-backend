using _116.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.EditArticleComment.V1;

/// <summary>
/// Integration tests for the PublicEditArticleComment endpoint.
/// </summary>
[Collection("Database")]
public class PublicEditArticleCommentEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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

    private async Task<ArticleCommentEntity> SeedCommentAsync(Guid articleId, Guid authorId)
    {
        return await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity comment = ArticleCommentFactory.Create(articleId, authorId);
            ctx.ArticleComments.Add(comment);
            return comment;
        });
    }

    [Fact]
    public async Task EditArticleComment_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        PublicEditArticleCommentRequest request = new PublicEditArticleCommentRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Public.Articles.Comment(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EditArticleComment_AsVisitor_NonExistentComment_ReturnsNotFound()
    {
        Client.AuthenticateAsVisitor();
        PublicEditArticleCommentRequest request = new PublicEditArticleCommentRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(
            Routes.Public.Articles.Comment(Guid.NewGuid(), Guid.NewGuid()),
            request
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that editing a comment that does not exist returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task EditComment_WhenNotExists_ReturnsNotFound()
    {
        ArticleEntity article = await SeedArticleAsync();
        Client.AuthenticateAsVisitor();
        PublicEditArticleCommentRequest request = new PublicEditArticleCommentRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Articles.Comment(article.Id, Guid.NewGuid()), request);

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that editing a comment owned by another user returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task EditComment_WhenNotOwner_ReturnsBadRequest()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedCommentAsync(article.Id, TestUser.SuperAdminId);

        Client.AuthenticateAsVisitor();
        PublicEditArticleCommentRequest request = new PublicEditArticleCommentRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Articles.Comment(article.Id, comment.Id), request);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that the owner can edit their own comment and the new body persists.
    /// </summary>
    [Fact]
    public async Task EditComment_AsOwner_UpdatesCommentBody()
    {
        ArticleEntity article = await SeedArticleAsync();
        ArticleCommentEntity comment = await SeedCommentAsync(article.Id, TestUser.VisitorId);

        Client.AuthenticateAsVisitor();
        PublicEditArticleCommentRequest request = new PublicEditArticleCommentRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync(Routes.Public.Articles.Comment(article.Id, comment.Id), request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PublicEditArticleCommentResponse body = await response.ReadAsAsync<PublicEditArticleCommentResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        ArticleCommentEntity? stored = await verifyDb.ArticleComments.FindAsync(comment.Id);
        stored!.Body.Should().Be(request.Body);
    }
}
