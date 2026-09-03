using _116.Content.Infrastructure.BackgroundJobs;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="ContentOutboxReplayJob" />: the job that re-dispatches the
/// Content module's undispatched events.
/// </summary>
public class ContentOutboxReplayJobTests
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
        var job = new ContentOutboxReplayJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ContentOutboxReplayJob>.Instance
        );

        // Assert
        job.Should().BeAssignableTo<OutboxReplayJob<ContentDbContext>>();
    }
}
