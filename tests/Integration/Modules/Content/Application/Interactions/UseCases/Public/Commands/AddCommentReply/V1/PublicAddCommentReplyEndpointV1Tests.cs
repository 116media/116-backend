using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;

/// <summary>
/// Integration tests for the PublicAddCommentReply endpoint.
/// </summary>
[Collection("Database")]
public class PublicAddCommentReplyEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
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
    public async Task AddReply_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(Guid.NewGuid(), Guid.NewGuid()),
            new PublicAddCommentReplyRequest("hello")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddReply_AsVisitor_CreatesReplyWithAuthorAndBumpsCommentCount()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedArticleWithCommentAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest("a valid reply body")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsAsync<PublicAddCommentReplyResponse>();
        body.Reply.ParentCommentId.Should().Be(parent.Id);
        body.Reply.Author.Should().NotBeNull();

        await using ContentDbContext verifyDb = CreateDbContext<ContentDbContext>();
        (await verifyDb.ArticleComments.CountAsync(c => c.ParentCommentId == parent.Id)).Should().Be(1);
    }

    [Fact]
    public async Task AddReply_ToAReply_ReturnsBadRequest()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedArticleWithCommentAsync();

        ArticleCommentEntity reply = await SeedAsync<ContentDbContext, ArticleCommentEntity>(ctx =>
        {
            ArticleCommentEntity r = ArticleCommentEntity.CreateReply(
                Guid.NewGuid(),
                TestUser.VisitorId,
                article.Id,
                parent.Id,
                "first reply"
            );
            ctx.ArticleComments.Add(r);
            return r;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, reply.Id),
            new PublicAddCommentReplyRequest("nested reply")
        );

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddReply_ToNonExistentParent_ReturnsNotFound()
    {
        (ArticleEntity article, _) = await SeedArticleWithCommentAsync();
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, Guid.NewGuid()),
            new PublicAddCommentReplyRequest("reply")
        );

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
