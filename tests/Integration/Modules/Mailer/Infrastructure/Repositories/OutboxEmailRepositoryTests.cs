using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Domain.Constants;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using _116.Mailer.Infrastructure.Persistence;

namespace _116.Integration.Tests.Modules.Mailer.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IOutboxEmailRepository" />'s claim: the dispatcher's batch
/// is taken durably in one statement, a concurrent run sees a disjoint batch, and a row whose
/// claim lapsed returns to the pool.
/// </summary>
[Collection("Database")]
public class OutboxEmailRepositoryTests(PostgresFixture db) : BaseRepositoryTest(db)
{
    [Fact]
    public async Task ClaimDueBatchAsync_ShouldStampTheClaimBeforeAnySendHappens()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        OutboxEmailEntity email = await SeedPendingEmailAsync(dueAt: now.AddMinutes(-1));

        var repository = Resolve<IOutboxEmailRepository>();

        // Act
        IReadOnlyList<OutboxEmailEntity> claimed = await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: now,
            leaseExpiresAt: now + MailerConstants.ClaimLease,
            cancellationToken: CancellationToken.None
        );

        // Assert
        claimed.Should().Contain(row => row.Id == email.Id);

        await using MailerDbContext context = CreateDbContext<MailerDbContext>();
        OutboxEmailEntity persisted = await context.OutboxEmails.SingleAsync(row => row.Id == email.Id);

        persisted.Status.Should().Be(EnumOutboxEmailStatus.Claimed);
        persisted.LeaseExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ClaimDueBatchAsync_WithAClaimStillHeld_ShouldNotHandTheRowToASecondDispatcher()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        OutboxEmailEntity email = await SeedPendingEmailAsync(dueAt: now.AddMinutes(-1));

        var repository = Resolve<IOutboxEmailRepository>();

        await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: now,
            leaseExpiresAt: now + MailerConstants.ClaimLease,
            cancellationToken: CancellationToken.None
        );

        // Act
        IReadOnlyList<OutboxEmailEntity> second = await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: now,
            leaseExpiresAt: now + MailerConstants.ClaimLease,
            cancellationToken: CancellationToken.None
        );

        // Assert
        second.Should().NotContain(row => row.Id == email.Id);
    }

    [Fact]
    public async Task ClaimDueBatchAsync_WithALapsedLease_ShouldReclaimTheRowADeadDispatcherHeld()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        OutboxEmailEntity email = await SeedPendingEmailAsync(dueAt: now.AddMinutes(-10));

        var repository = Resolve<IOutboxEmailRepository>();

        DateTime crashTime = now.AddMinutes(-5);
        await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: crashTime,
            leaseExpiresAt: crashTime + MailerConstants.ClaimLease,
            cancellationToken: CancellationToken.None
        );

        // Act
        IReadOnlyList<OutboxEmailEntity> reclaimed = await repository.ClaimDueBatchAsync(
            batchSize: MailerConstants.DispatchBatchSize,
            now: now,
            leaseExpiresAt: now + MailerConstants.ClaimLease,
            cancellationToken: CancellationToken.None
        );

        // Assert
        reclaimed.Should().Contain(row => row.Id == email.Id);
    }

    /// <summary>
    /// Writes one pending outbox email due at the supplied moment.
    /// </summary>
    /// <param name="dueAt">When the row becomes eligible for delivery.</param>
    /// <returns>The persisted row.</returns>
    private async Task<OutboxEmailEntity> SeedPendingEmailAsync(DateTime dueAt)
    {
        await using MailerDbContext context = CreateDbContext<MailerDbContext>();

        OutboxEmailEntity email = OutboxEmailEntity.Enqueue(
            id: Guid.NewGuid(),
            recipientAddress: $"claim-{Guid.NewGuid():N}@test.com",
            recipientName: "Fan",
            subject: "subject",
            htmlBody: "<p>body</p>",
            textBody: "body",
            template: "Welcome",
            now: dueAt
        );

        context.OutboxEmails.Add(email);
        await context.SaveChangesAsync();

        return email;
    }
}
