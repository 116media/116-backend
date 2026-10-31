using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.BackgroundJobs;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace _116.Content.Unit.Tests.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="SearchProjectionReconcileJob" />.
/// </summary>
public class SearchProjectionReconcileJobTests
{
    private readonly Mock<ISearchProjection> _projectionMock;
    private readonly Mock<IJobExecutionContext> _jobContextMock;
    private readonly SearchProjectionReconcileJob _job;

    public SearchProjectionReconcileJobTests()
    {
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<ILogger<SearchProjectionReconcileJob>> loggerMock = new();

        _projectionMock = new Mock<ISearchProjection>();
        _jobContextMock = new Mock<IJobExecutionContext>();

        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(ISearchProjection))).Returns(_projectionMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _job = new SearchProjectionReconcileJob(scopeFactoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldRebuildEverySearchableType()
    {
        // Arrange
        _projectionMock
            .Setup(p => p.RebuildAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumCoreContentType, int>());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _projectionMock.Verify(p => p.RebuildAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldPassTheJobCancellationToken()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        _jobContextMock.Setup(x => x.CancellationToken).Returns(source.Token);
        _projectionMock
            .Setup(p => p.RebuildAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumCoreContentType, int>());

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _projectionMock.Verify(p => p.RebuildAllAsync(source.Token), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenDocumentsWereRebuilt_ShouldCompleteNormally()
    {
        // Arrange
        _projectionMock
            .Setup(p => p.RebuildAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<EnumCoreContentType, int>
                {
                    [EnumCoreContentType.Article] = 12,
                    [EnumCoreContentType.Video] = 7,
                    [EnumCoreContentType.Short] = 3,
                }
            );

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Execute_WhenTheProjectionThrows_ShouldSwallowTheFailure()
    {
        // Arrange
        _projectionMock
            .Setup(p => p.RebuildAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("connection lost"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync("a scheduled job must not take the scheduler down");
    }
}
