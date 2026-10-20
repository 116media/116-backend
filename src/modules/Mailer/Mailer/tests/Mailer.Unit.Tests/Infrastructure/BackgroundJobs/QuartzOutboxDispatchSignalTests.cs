using _116.Mailer.Infrastructure.BackgroundJobs;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Xunit;

namespace _116.Mailer.Unit.Tests.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="QuartzOutboxDispatchSignal" />. The signal is an optimisation over
/// the dispatcher's cron, so a scheduler that refuses must cost latency rather than the caller.
/// </summary>
public class QuartzOutboxDispatchSignalTests
{
    private readonly Mock<ISchedulerFactory> _schedulerFactoryMock = new();
    private readonly Mock<IScheduler> _schedulerMock = new();
    private readonly QuartzOutboxDispatchSignal _signal;

    public QuartzOutboxDispatchSignalTests()
    {
        _schedulerFactoryMock
            .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_schedulerMock.Object);

        _signal = new QuartzOutboxDispatchSignal(
            schedulerFactory: _schedulerFactoryMock.Object,
            logger: NullLogger<QuartzOutboxDispatchSignal>.Instance
        );
    }

    [Fact]
    public async Task RequestDispatchAsync_ShouldTriggerTheDispatcherJob()
    {
        // Act
        await _signal.RequestDispatchAsync(CancellationToken.None);

        // Assert
        _schedulerMock.Verify(
            x =>
                x.TriggerJob(
                    It.Is<JobKey>(key => key.Name == nameof(OutboxEmailDispatcherJob)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RequestDispatchAsync_WhenTheSchedulerRefuses_ShouldNotFailTheCaller()
    {
        // Arrange
        _schedulerMock
            .Setup(x => x.TriggerJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SchedulerException("scheduler is shutting down"));

        // Act
        Func<Task> act = async () => await _signal.RequestDispatchAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequestDispatchAsync_WhenTheSchedulerCannotBeResolved_ShouldNotFailTheCaller()
    {
        // Arrange
        _schedulerFactoryMock
            .Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SchedulerException("no scheduler"));

        // Act
        Func<Task> act = async () => await _signal.RequestDispatchAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
