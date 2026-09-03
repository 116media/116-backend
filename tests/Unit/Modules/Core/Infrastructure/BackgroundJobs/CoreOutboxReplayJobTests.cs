using _116.Core.Infrastructure.BackgroundJobs;
using _116.Core.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="CoreOutboxReplayJob" />: the job that re-dispatches the
/// Core module's undispatched events.
/// </summary>
public class CoreOutboxReplayJobTests
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
        var job = new CoreOutboxReplayJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<CoreOutboxReplayJob>.Instance
        );

        // Assert
        job.Should().BeAssignableTo<OutboxReplayJob<CoreDbContext>>();
    }
}
