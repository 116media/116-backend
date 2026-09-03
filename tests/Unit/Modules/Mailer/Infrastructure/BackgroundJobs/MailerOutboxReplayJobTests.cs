using _116.Mailer.Infrastructure.BackgroundJobs;
using _116.Mailer.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Outbox;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="MailerOutboxReplayJob" />: the job that re-dispatches the
/// Mailer module's undispatched events.
/// </summary>
public class MailerOutboxReplayJobTests
{
    [Fact]
    public void Constructor_ShouldBindTheReplayJobToTheModulesOwnContext()
    {
        // Arrange
        var services = new ServiceCollection();
        ServiceProvider provider = services.BuildServiceProvider();

        // Act
        var job = new MailerOutboxReplayJob(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<MailerOutboxReplayJob>.Instance
        );

        // Assert
        job.Should().BeAssignableTo<OutboxReplayJob<MailerDbContext>>();
    }
}
