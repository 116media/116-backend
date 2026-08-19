using _116.Content.Application.Shared.Repositories;
using _116.Content.Infrastructure.BackgroundJobs;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="ShortVideoViewEventCleanupJob" />.
/// </summary>
public class ShortVideoViewEventCleanupJobTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IJobExecutionContext> _jobContextMock;
    private readonly ShortVideoViewEventCleanupJob _job;

    public ShortVideoViewEventCleanupJobTests()
    {
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<ILogger<ShortVideoViewEventCleanupJob>> loggerMock = new();

        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _jobContextMock = new Mock<IJobExecutionContext>();

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IShortVideoRepository)))
            .Returns(_shortVideoRepositoryMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _job = new ShortVideoViewEventCleanupJob(scopeFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldPruneWithACutoffInThePast()
    {
        // Arrange
        _shortVideoRepositoryMock
            .Setup(r => r.PruneUncountedViewEventsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _shortVideoRepositoryMock.Verify(
            r =>
                r.PruneUncountedViewEventsAsync(
                    It.Is<DateTime>(c => c < DateTime.UtcNow),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_WhenEventsWerePruned_ShouldCompleteNormally()
    {
        // Arrange
        _shortVideoRepositoryMock
            .Setup(r => r.PruneUncountedViewEventsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Execute_WhenTheRepositoryThrows_ShouldSwallowTheFailure()
    {
        // Arrange — a Quartz job must never surface an exception into the scheduler
        _shortVideoRepositoryMock
            .Setup(r => r.PruneUncountedViewEventsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("connection lost"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
