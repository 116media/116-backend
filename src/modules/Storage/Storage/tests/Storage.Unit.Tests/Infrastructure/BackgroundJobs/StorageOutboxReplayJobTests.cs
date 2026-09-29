using _116.BuildingBlocks.Infrastructure.Outbox;
using _116.Storage.Infrastructure.BackgroundJobs;
using _116.Storage.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Storage.Unit.Tests.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="StorageOutboxReplayJob" />: the job that re-dispatches the
/// Storage module's undispatched events.
/// </summary>
public class StorageOutboxReplayJobTests
{
    [Fact]
    public void Constructor_ShouldBindTheReplayJobToTheModulesOwnContext()
    {
        // Arrange
        // The context type parameter decides which module's outbox is drained; binding the wrong
        // one would leave this module's events undelivered forever.
        var services = new ServiceCollection();
        ServiceProvider provider = services.BuildServiceProvider();

        // Act
        var job = new StorageOutboxReplayJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<StorageOutboxReplayJob>.Instance
        );

        // Assert
        job.Should().BeAssignableTo<OutboxReplayJob<StorageDbContext>>();
    }
}
