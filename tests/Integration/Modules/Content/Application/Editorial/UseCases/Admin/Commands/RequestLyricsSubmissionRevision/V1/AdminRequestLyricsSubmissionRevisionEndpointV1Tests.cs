using _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision.V1;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision.V1;

/// <summary>
/// Integration tests for the AdminRequestLyricsSubmissionRevision endpoint.
/// </summary>
[Collection("Database")]
public class AdminRequestLyricsSubmissionRevisionEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task RequestLyricsSubmissionRevision_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RequestSubmissionRevision(Guid.NewGuid()),
            new AdminRequestLyricsSubmissionRevisionRequest("Please fix formatting.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLyricsSubmissionRevision_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RequestSubmissionRevision(Guid.NewGuid()),
            new AdminRequestLyricsSubmissionRevisionRequest("Please fix formatting.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Requesting a revision on a pending submission sets its status to <c>NeedsRevision</c>
    /// with the moderator's note, without creating a <see cref="LyricsEntity" /> and without
    /// rejecting the submission outright.
    /// </summary>
    [Fact]
    public async Task RequestLyricsSubmissionRevision_HappyPath_SetsNeedsRevisionStatusAndNote()
    {
        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
            ctx.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RequestSubmissionRevision(submission.Id),
            new AdminRequestLyricsSubmissionRevisionRequest("Please fix formatting.")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsSubmissionEntity? persisted = await ctx.LyricsSubmissions.FindAsync(submission.Id);

        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(EnumSubmissionStatus.NeedsRevision);
        persisted.ReviewNote.Should().Be("Please fix formatting.");
        persisted.ReviewedByUserId.Should().Be(TestUser.AdminId);
        persisted.PublishedLyricsId.Should().BeNull();

        bool anyLyricsCreated = await ctx.Lyrics.AnyAsync();
        anyLyricsCreated.Should().BeFalse();
    }

    /// <summary>
    /// A revision request is a decision like approval and rejection, so it reaches a resolvable
    /// submitter on both channels. Its wording is neither of the two terminal outcomes: the copy
    /// says the submission was returned for revision, and the moderator's note rides the email.
    /// </summary>
    [Fact]
    public async Task RequestLyricsSubmissionRevision_ForAKnownSubmitter_TellsThemItWasReturnedForRevision()
    {
        const string note = "Please split the chorus onto its own lines.";

        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(ctx =>
        {
            LyricsSubmissionEntity created = LyricsSubmissionFactory.Create(TestUser.VisitorId);
            ctx.LyricsSubmissions.Add(created);
            return created;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Lyrics.RequestSubmissionRevision(submission.Id),
            new AdminRequestLyricsSubmissionRevisionRequest(note)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using MailerDbContext mailerContext = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity email = (
            await mailerContext.OutboxEmails.Where(o => o.RecipientAddress == TestUser.VisitorEmail).ToListAsync()
        )
            .Should()
            .ContainSingle(o => o.Template == "SubmissionDecided")
            .Subject;

        email.TextBody.Should().Contain("returned for revision");
        email.TextBody.Should().Contain(note);

        List<NotificationEntity> notifications = await mailerContext
            .Notifications.Where(n => n.UserId == TestUser.VisitorId)
            .ToListAsync();
        notifications.Should().ContainSingle(n => n.Type == EnumNotificationType.SubmissionDecided);
    }
}
