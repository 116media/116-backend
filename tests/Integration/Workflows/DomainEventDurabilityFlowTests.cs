using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Workflows;

/// <summary>
/// Proof that an event raised inside an explicit transaction survives the dispatch that never
/// ran: the outbox row is committed with the state change, the replay job delivers it, and a
/// second delivery attempt produces no duplicate.
/// </summary>
[Collection("Database")]
public class DomainEventDurabilityFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ApproveSubmission_RaisedInATransaction_IsDeliveredExactlyOnceByReplay()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        string submitterEmail = $"durability-{submitterId:N}@test.com";

        await SeedAsync<IdentityDbContext>(context =>
        {
            var submitter = UserFactory.CreateWithId(submitterId, submitterEmail);
            submitter.MarkAsVerified();
            submitter.Activate();
            context.Users.Add(submitter);
        });

        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(context =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create(submitterId);

            context.ContentTypes.Add(contentType);
            context.Categories.Add(category);
            context.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        // Act
        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest($"durable-{submission.Id:N}")
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The handler commits through one transaction, so in-process dispatch is skipped and the
        // event exists only as a durable, undelivered outbox row.
        OutboxEventEntity row = await SingleDecisionRowAsync(submission.Id);
        row.DispatchedAt.Should().BeNull();

        (await SentEmailCountAsync(submitterEmail)).Should().Be(0);

        // Act
        await RunContentReplayAsync();

        // Assert
        (await SingleDecisionRowAsync(submission.Id))
            .DispatchedAt.Should()
            .NotBeNull();
        (await SentEmailCountAsync(submitterEmail)).Should().Be(1);
    }

    [Fact]
    public async Task ReplayingADeliveredEvent_ShouldNotSendTheEmailTwice()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        string submitterEmail = $"idempotent-{submitterId:N}@test.com";

        await SeedAsync<IdentityDbContext>(context =>
        {
            var submitter = UserFactory.CreateWithId(submitterId, submitterEmail);
            submitter.MarkAsVerified();
            submitter.Activate();
            context.Users.Add(submitter);
        });

        LyricsSubmissionEntity submission = await SeedAsync<ContentDbContext, LyricsSubmissionEntity>(context =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(contentType.Id);
            LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create(submitterId);

            context.ContentTypes.Add(contentType);
            context.Categories.Add(category);
            context.LyricsSubmissions.Add(submission);
            return submission;
        });

        Client.AuthenticateAsAdmin();

        var response = await Client.PutAsJsonAsync(
            Routes.Admin.Lyrics.Submission(submission.Id),
            new AdminApproveLyricsSubmissionRequest($"idempotent-{submission.Id:N}")
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await RunContentReplayAsync();
        (await SentEmailCountAsync(submitterEmail)).Should().Be(1);

        // Act
        // Clearing the dispatch stamp is what a dispatcher that died before recording its outcome
        // leaves behind, so replay picks the row up again and only the processed-event guard can
        // keep the second delivery from happening.
        await ResetDispatchStampAsync(submission.Id);
        await RunContentReplayAsync();

        // Assert
        (await SentEmailCountAsync(submitterEmail))
            .Should()
            .Be(1);
    }

    /// <summary>
    /// Runs the Content module's replay job once through its real entry point.
    /// </summary>
    private async Task RunContentReplayAsync()
    {
        using IServiceScope scope = Api.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<ContentOutboxReplayJob>();

        await job.Execute(new TestJobExecutionContext());
    }

    /// <summary>
    /// Reads the single outbox row carrying the decision event for a submission.
    /// </summary>
    /// <param name="submissionId">The submission whose decision event is expected.</param>
    /// <returns>The one matching outbox row.</returns>
    private async Task<OutboxEventEntity> SingleDecisionRowAsync(Guid submissionId)
    {
        await using ContentDbContext context = CreateDbContext<ContentDbContext>();

        List<OutboxEventEntity> rows = await context
            .Set<OutboxEventEntity>()
            .Where(row => row.EventType.Contains("LyricsSubmissionDecidedEvent"))
            .Where(row => row.Payload.Contains(submissionId.ToString()))
            .ToListAsync();

        rows.Should().ContainSingle("the approval raises exactly one decision event");
        return rows[0];
    }

    /// <summary>
    /// Returns the row to the state a dispatcher that died before recording its outcome leaves.
    /// </summary>
    /// <param name="submissionId">The submission whose decision event is replayed.</param>
    private async Task ResetDispatchStampAsync(Guid submissionId)
    {
        OutboxEventEntity row = await SingleDecisionRowAsync(submissionId);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();

        await context
            .Set<OutboxEventEntity>()
            .Where(candidate => candidate.Id == row.Id)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(candidate => candidate.DispatchedAt, (DateTime?)null)
                    .SetProperty(candidate => candidate.AttemptCount, 0)
            );
    }

    /// <summary>
    /// Counts the decision emails queued for a recipient.
    /// </summary>
    /// <param name="recipientAddress">The submitter's address.</param>
    /// <returns>How many outbox emails target that address.</returns>
    private async Task<int> SentEmailCountAsync(string recipientAddress)
    {
        await using MailerDbContext context = CreateDbContext<MailerDbContext>();

        return await context.OutboxEmails.CountAsync(email => email.RecipientAddress == recipientAddress);
    }
}
