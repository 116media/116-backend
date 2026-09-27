using _116.BuildingBlocks.Infrastructure.Outbox;
using _116.Identity.Infrastructure.BackgroundJobs;
using _116.Identity.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Identity.Unit.Tests.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="IdentityOutboxReplayJob" />: the job that re-dispatches the
/// Identity module's undispatched events.
/// </summary>
public class IdentityOutboxReplayJobTests
{
    [Fact]
    public void Constructor_ShouldBindTheReplayJobToTheModulesOwnContext()
    {
        // Arrange
        var services = new ServiceCollection();
        ServiceProvider provider = services.BuildServiceProvider();

        // Act
        var job = new IdentityOutboxReplayJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<IdentityOutboxReplayJob>.Instance
        );

        // Assert
        job.Should().BeAssignableTo<OutboxReplayJob<IdentityDbContext>>();
    }
}
