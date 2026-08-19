using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
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

    /// <summary>
    /// Seeds a published article carrying one top-level comment written by the given user, so a
    /// reply from a different account reaches the notification path instead of the self-reply
    /// short circuit.
    /// </summary>
    /// <param name="authorUserId">The identity user UUID credited with the parent comment.</param>
    /// <returns>The seeded article and its parent comment.</returns>
    private async Task<(ArticleEntity Article, ArticleCommentEntity Parent)> SeedArticleWithCommentByAsync(
        Guid authorUserId
    )
    {
        return await SeedAsync<ContentDbContext, (ArticleEntity, ArticleCommentEntity)>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ctx.Categories.Add(category);
            ArticleEntity article = ArticleFactory.CreatePublished(category.Id);
            ctx.Articles.Add(article);
            ArticleCommentEntity parent = ArticleCommentFactory.Create(article.Id, authorUserId);
            ctx.ArticleComments.Add(parent);
            return (article, parent);
        });
    }

    /// <summary>
    /// A reply body past the excerpt limit reaches the parent author's email cut back to the last
    /// word boundary before the limit, with an ellipsis standing in for the remainder. The tail of
    /// the body must not survive into the notice.
    /// </summary>
    [Fact]
    public async Task AddReply_WithALongBody_TruncatesTheEmailExcerptAtAWordBoundary()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedArticleWithCommentByAsync(TestUser.AdminId);

        string head = new('a', 100);
        string middle = new('b', 60);
        const string tail = "TAILMARKER";
        string body = $"{head} {middle} {tail}";

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest(body)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity email = (
            await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == TestUser.AdminEmail).ToListAsync()
        )
            .Should()
            .ContainSingle(o => o.Template == "CommentReply")
            .Subject;

        email.TextBody.Should().Contain($"{head}…");
        email.TextBody.Should().NotContain(tail);
        email.TextBody.Should().NotContain(middle);
    }

    /// <summary>
    /// A long reply body holding no whitespace before the excerpt limit has no word boundary to
    /// fall back on, so the excerpt is cut at the limit itself and still carries the ellipsis.
    /// </summary>
    [Fact]
    public async Task AddReply_WithALongUnbrokenBody_TruncatesTheEmailExcerptAtTheLimit()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedArticleWithCommentByAsync(TestUser.AdminId);

        const int excerptLimit = 140;
        string unbroken = new('z', 160);
        string body = $"{unbroken} TAILMARKER";

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest(body)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity email = (
            await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == TestUser.AdminEmail).ToListAsync()
        )
            .Should()
            .ContainSingle(o => o.Template == "CommentReply")
            .Subject;

        email.TextBody.Should().Contain($"{new string('z', excerptLimit)}…");
        email.TextBody.Should().NotContain(new string('z', excerptLimit + 1));
    }
}
