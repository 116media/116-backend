using _116.Core.Infrastructure.BackgroundJobs;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Infrastructure.BackgroundJobs;
using _116.Integration.Tests.Common.Stubs;
using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Shared.Application.Jobs;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Outbox;

namespace _116.Integration.Tests.Shared.Infrastructure.Outbox;

/// <summary>
/// Integration tests for the per-module replay jobs. Each module schedules its own, so each has
/// to drain its own outbox and cope with an empty one.
/// </summary>
[Collection("Database")]
public class OutboxReplayJobTests(PostgresFixture db) : BaseRepositoryTest(db)
{
    /// <summary>
    /// An event no handler is registered for, so replay exercises delivery without side effects.
    /// </summary>
    /// <param name="Marker">A value distinguishing one seeded row from another.</param>
    private record UnhandledReplayEvent(Guid Marker) : DomainEvent;

    /// <summary>
    /// Runs one module's replay job through its real entry point.
    /// </summary>
    /// <typeparam name="TJob">The module job to run.</typeparam>
    private async Task RunAsync<TJob>()
        where TJob : notnull, IScheduledJob
    {
        using IServiceScope scope = Api.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<TJob>();

        await job.Execute(new TestJobExecutionContext());
    }

    [Fact]
    public async Task Execute_WithAnEmptyOutbox_ShouldCompleteForEveryModule()
    {
        // Arrange
        // Most runs find nothing; a module whose job threw on an empty outbox would fill the
        // logs and never drain a real backlog.
        // Act
        Func<Task> act = async () =>
        {
            await RunAsync<CoreOutboxReplayJob>();
            await RunAsync<IdentityOutboxReplayJob>();
            await RunAsync<MailerOutboxReplayJob>();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Execute_WithAnUndispatchedRow_ShouldMarkItDispatched()
    {
        // Arrange
        // This is the state a dispatcher that died after the commit leaves behind.
        var marker = Guid.NewGuid();
        OutboxEventEntity row = OutboxEventEntity.Create(new UnhandledReplayEvent(marker));

        await using (CoreDbContext seed = CreateDbContext<CoreDbContext>())
        {
            seed.Set<OutboxEventEntity>().Add(row);
            await seed.SaveChangesAsync();
        }

        // Act
        await RunAsync<CoreOutboxReplayJob>();

        // Assert
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();
        OutboxEventEntity replayed = await context.Set<OutboxEventEntity>().SingleAsync(r => r.Id == row.Id);
        replayed.DispatchedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_WithAnAlreadyDispatchedRow_ShouldLeaveItUntouched()
    {
        // Arrange
        // Replay scans on the dispatch stamp, so a retired row must stay out of the batch.
        OutboxEventEntity row = OutboxEventEntity.Create(new UnhandledReplayEvent(Guid.NewGuid()));
        row.MarkDispatched();
        DateTime? dispatchedAt = row.DispatchedAt;

        await using (CoreDbContext seed = CreateDbContext<CoreDbContext>())
        {
            seed.Set<OutboxEventEntity>().Add(row);
            await seed.SaveChangesAsync();
        }

        // Act
        await RunAsync<CoreOutboxReplayJob>();

        // Assert
        await using CoreDbContext context = CreateDbContext<CoreDbContext>();
        OutboxEventEntity untouched = await context.Set<OutboxEventEntity>().SingleAsync(r => r.Id == row.Id);
        untouched.DispatchedAt.Should().BeCloseTo(dispatchedAt!.Value, TimeSpan.FromSeconds(1));
    }
}
