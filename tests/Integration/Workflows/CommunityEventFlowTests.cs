using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision.V1;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision.V1;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission.V1;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.VerifyArtistOwner.V1;
using _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Mailer.Contracts.Domain;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// End-to-end flows for the community reactions hosted by domain event handlers: revision and
/// submission decisions reach the proposer/submitter over both channels (with the moderator
/// note landing in the email body), an artist claim request persists a durable row, ownership
/// verification congratulates the new owner, and a comment reply notifies the parent author.
/// </summary>
[Collection("Database")]
public class CommunityEventFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RejectLyricsSubmission_OverRealHttp_DeliversTheModeratorNoteToTheSubmitter()
    {
        const string note = "Les paroles contiennent des erreurs de transcription.";
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            LyricsSubmissionEntity created = LyricsSubmissionFactory.Create(TestUser.VisitorId);
            ctx.LyricsSubmissions.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RejectSubmission(submission.Id),
            new AdminRejectLyricsSubmissionRequest(note)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        OutboxEmailEntity email = outbox.Should().ContainSingle(o => o.Template == "SubmissionDecided").Subject;
        email.TextBody.Should().Contain(note);
        email.TextBody.Should().Contain(submission.SongTitle);

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.SubmissionDecided);
    }

    [Fact]
    public async Task ApproveLyricsSubmission_OverRealHttp_LinksTheNotificationToThePublishedPage()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsSubmissionEntity created = LyricsSubmissionFactory.Create(TestUser.VisitorId);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.LyricsSubmissions.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest("eloko-oyo-approved")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        outbox.Should().ContainSingle(o => o.Template == "SubmissionDecided");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        NotificationEntity notification = notifications
            .Should()
            .ContainSingle(n => n.Type == EnumNotificationType.SubmissionDecided)
            .Subject;
        notification.LinkPath.Should().Be("/lyrics/eloko-oyo-approved");
    }

    [Fact]
    public async Task DecideLyricsRevision_AcceptOverRealHttp_NotifiesTheProposerOnBothChannels()
    {
        LyricsRevisionEntity revision = await SeedAsync<ContentDbContext, LyricsRevisionEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
            LyricsRevisionEntity created = LyricsRevisionFactory.Create(
                lyrics.Id,
                TestUser.VisitorId,
                "Corrected community text"
            );
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(lyrics);
            ctx.LyricsRevisions.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Revision(revision.Id),
            new AdminDecideLyricsRevisionRequest(true)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        OutboxEmailEntity email = outbox.Should().ContainSingle(o => o.Template == "RevisionDecided").Subject;
        email.TextBody.Should().Contain("accepted");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.RevisionDecided);
    }

    [Fact]
    public async Task DecideTranslationRevision_RejectOverRealHttp_NotifiesTheProposerOnBothChannels()
    {
        LyricsTranslationRevisionEntity revision = await SeedAsync<ContentDbContext, LyricsTranslationRevisionEntity>(
            ctx =>
            {
                ContentTypeEntity contentType = ContentTypeFactory.Create();
                CategoryEntity category = CategoryFactory.Create(contentType.Id);
                LyricsEntity lyrics = LyricsFactory.CreatePublished(category.Id);
                LyricsTranslationEntity translation = LyricsTranslationFactory.CreateWithText(
                    lyrics.Id,
                    "es",
                    "Original text"
                );
                LyricsTranslationRevisionEntity created = LyricsTranslationRevisionFactory.Create(
                    translation.Id,
                    TestUser.VisitorId,
                    "Proposed text"
                );
                ctx.ContentTypes.Add(contentType);
                ctx.Categories.Add(category);
                ctx.Lyrics.Add(lyrics);
                ctx.LyricsTranslations.Add(translation);
                ctx.LyricsTranslationRevisions.Add(created);
                return created;
            }
        );

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Translations.Revision(revision.Id),
            new AdminDecideTranslationRevisionRequest(false)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        OutboxEmailEntity email = outbox.Should().ContainSingle(o => o.Template == "RevisionDecided").Subject;
        email.TextBody.Should().Contain("rejected");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.RevisionDecided);
    }

    [Fact]
    public async Task RequestArtistClaim_OverRealHttp_PersistsADurableClaimRequestRow()
    {
        ArtistEntity artist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity created = ArtistFactory.Create();
            ctx.Artists.Add(created);
            return created;
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(Routes.Public.Artists.Claim(artist.Id), new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext contentContext = CreateDbContext<ContentDbContext>();
        List<ArtistClaimRequestEntity> requests = await contentContext
            .ArtistClaimRequests.Where(r => r.ArtistId == artist.Id)
            .ToListAsync();
        requests.Should().ContainSingle().Which.UserId.Should().Be(TestUser.VisitorId);

        ArtistEntity? persistedArtist = await contentContext.Artists.FindAsync(artist.Id);
        persistedArtist!.UserId.Should().BeNull();
    }

    [Fact]
    public async Task VerifyArtistOwner_OverRealHttp_CongratulatesTheNewOwnerOnBothChannels()
    {
        ArtistEntity artist = await SeedAsync<ContentDbContext, ArtistEntity>(ctx =>
        {
            ArtistEntity created = ArtistFactory.Create();
            ctx.Artists.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PostAsJsonAsync(
            Routes.Admin.Artists.VerifyOwner(artist.Id),
            new AdminVerifyArtistOwnerRequest(TestUser.VisitorId)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail)
            .ToListAsync();
        OutboxEmailEntity email = outbox.Should().ContainSingle(o => o.Template == "ArtistVerified").Subject;
        email.TextBody.Should().Contain(artist.Name);

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        NotificationEntity notification = notifications
            .Should()
            .ContainSingle(n => n.Type == EnumNotificationType.ArtistVerified)
            .Subject;
        notification.LinkPath.Should().Be($"/artists/{artist.Slug}");
    }

    [Fact]
    public async Task AddCommentReply_OverRealHttp_NotifiesTheParentAuthorOnBothChannels()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedAsync<
            ContentDbContext,
            (ArticleEntity, ArticleCommentEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            ArticleCommentEntity parentComment = ArticleCommentFactory.Create(created.Id, TestUser.AdminId);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(created);
            ctx.ArticleComments.Add(parentComment);
            return (created, parentComment);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest("Totally agree with this take!")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        var outbox = await mailerContext
            .OutboxEmails.Where(o => o.RecipientAddress == TestUser.AdminEmail)
            .ToListAsync();
        OutboxEmailEntity email = outbox.Should().ContainSingle(o => o.Template == "CommentReply").Subject;
        email.TextBody.Should().Contain("Totally agree with this take!");

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.AdminId)
            .ToListAsync();
        NotificationEntity notification = notifications
            .Should()
            .ContainSingle(n => n.Type == EnumNotificationType.CommentReply)
            .Subject;
        notification.LinkPath.Should().Be($"/articles/{article.Slug}");
    }

    [Fact]
    public async Task AddCommentReply_ToOwnComment_OverRealHttp_NotifiesNobody()
    {
        (ArticleEntity article, ArticleCommentEntity parent) = await SeedAsync<
            ContentDbContext,
            (ArticleEntity, ArticleCommentEntity)
        >(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            ArticleEntity created = ArticleFactory.CreatePublished(category.Id);
            ArticleCommentEntity parentComment = ArticleCommentFactory.Create(created.Id, TestUser.VisitorId);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Articles.Add(created);
            ctx.ArticleComments.Add(parentComment);
            return (created, parentComment);
        });

        Client.AuthenticateAsVisitor();

        var response = await Client.PostAsJsonAsync(
            Routes.Public.Articles.CommentReplies(article.Id, parent.Id),
            new PublicAddCommentReplyRequest("Replying to myself.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        bool anyReplyEmail = await mailerContext.OutboxEmails.AnyAsync(o => o.Template == "CommentReply");
        anyReplyEmail.Should().BeFalse();

        bool anyReplyNotification = await mailerContext.Notifications.AnyAsync(n =>
            n.Type == EnumNotificationType.CommentReply
        );
        anyReplyNotification.Should().BeFalse();
    }
}
