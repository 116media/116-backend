using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;
using _116.Content.Application.Shared.OutboundEmails;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end proof of the two rendering rules a queued notification email must obey:
/// template syntax typed by a user is delivered literally rather than resolved, and the copy
/// renders in the recipient's own locale rather than the acting request's.
/// </summary>
[Collection("Database")]
public class NotificationRenderingFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    /// <summary>
    /// Seeds a verified article author who owns a top-level comment, so a reply from someone
    /// else reaches the comment-reply notification handler.
    /// </summary>
    /// <param name="locale">The locale the recipient's copy must render in.</param>
    /// <returns>The seeded article, the parent comment and the author's address.</returns>
    private async Task<(ArticleEntity Article, ArticleCommentEntity Parent, string Email)> SeedCommentAuthorAsync(
        string locale
    )
    {
        var authorId = Guid.NewGuid();
        string email = $"reply-target-{authorId:N}@test.com";

        await SeedAsync<IdentityDbContext>(context =>
        {
            UserEntity author = UserFactory.CreateWithId(authorId, email);
            author.MarkAsVerified();
            author.Activate();
            author.SetPreferredLocale(locale);

            context.Users.Add(author);
        });

        ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(context =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            context.ContentTypes.Add(contentType);
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            context.Categories.Add(category);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            context.Articles.Add(created);

            return created;
        });

        ArticleCommentEntity parent = await SeedAsync<ContentDbContext, ArticleCommentEntity>(context =>
        {
            ArticleCommentEntity created = ArticleCommentFactory.Create(article.Id, authorId);
            context.ArticleComments.Add(created);

            return created;
        });

        return (article, parent, email);
    }

    /// <summary>
    /// Reads the single comment-reply outbox row queued for an address.
    /// </summary>
    /// <param name="email">The recipient address.</param>
    /// <returns>The queued row.</returns>
    private async Task<OutboxEmailEntity> CommentReplyRowAsync(string email)
    {
        await using MailerDbContext mailer = CreateDbContext<MailerDbContext>();

        List<OutboxEmailEntity> rows = await mailer
            .OutboxEmails.Where(row => row.RecipientAddress == email)
            .ToListAsync();

        return rows.Should().ContainSingle(row => row.Template == ContentEmailTemplates.CommentReply).Which;
    }

    [Fact]
    public async Task CommentReply_WhoseBodyContainsTemplateSyntax_StillQueuesAFullyRenderedEmail()
    {
        (ArticleEntity article, ArticleCommentEntity parent, string email) = await SeedCommentAuthorAsync("en");
        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest("Great point about {{articleTitle}} honestly")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        OutboxEmailEntity queued = await CommentReplyRowAsync(email);

        queued.TextBody.Should().Contain("{{articleTitle}}", "user text is delivered literally");
        queued.HtmlBody.Should().Contain("{{articleTitle}}");
        queued.TextBody.Should().Contain(article.Title, "the template's own placeholder still resolves");
        queued.Subject.Should().NotBeNullOrWhiteSpace().And.NotContain("{{");
    }

    [Fact]
    public async Task CommentReply_ShouldRenderInTheRecipientsLocaleNotTheRequestCulture()
    {
        (ArticleEntity frenchArticle, ArticleCommentEntity frenchParent, string frenchEmail) =
            await SeedCommentAuthorAsync("fr");
        (ArticleEntity englishArticle, ArticleCommentEntity englishParent, string englishEmail) =
            await SeedCommentAuthorAsync("en");

        Client.AuthenticateAsVisitor();
        Client.DefaultRequestHeaders.Remove("Accept-Language");
        Client.DefaultRequestHeaders.Add("Accept-Language", "en");

        var toFrench = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(frenchArticle.Id, frenchParent.Id),
            new PublicAddCommentReplyRequest("a valid reply body")
        );
        var toEnglish = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(englishArticle.Id, englishParent.Id),
            new PublicAddCommentReplyRequest("a valid reply body")
        );

        toFrench.StatusCode.Should().Be(HttpStatusCode.Created);
        toEnglish.StatusCode.Should().Be(HttpStatusCode.Created);

        OutboxEmailEntity french = await CommentReplyRowAsync(frenchEmail);
        OutboxEmailEntity english = await CommentReplyRowAsync(englishEmail);

        french.Subject.Should().Contain("a répondu").And.NotBe(english.Subject);
        english.Subject.Should().Contain("replied to");
    }
}
