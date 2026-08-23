using _116.Identity.Application.Auth.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Infrastructure.BackgroundJobs;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="ExpiredOtpCleanupJob" />.
/// </summary>
public class ExpiredOtpCleanupJobTests
{
    private readonly Mock<IOtpRepository> _otpRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJobExecutionContext> _jobContextMock;
    private readonly ExpiredOtpCleanupJob _job;

    public ExpiredOtpCleanupJobTests()
    {
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        Mock<ILogger<ExpiredOtpCleanupJob>> loggerMock = new();

        _otpRepositoryMock = MockOtpRepository.Create();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _jobContextMock = new Mock<IJobExecutionContext>();

        // Wire up scope factory → scope → service provider → services
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        serviceProviderMock.Setup(x => x.GetService(typeof(IOtpRepository))).Returns(_otpRepositoryMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(IIdentityUnitOfWork))).Returns(_unitOfWorkMock.Object);

        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _job = new ExpiredOtpCleanupJob(scopeFactoryMock.Object, loggerMock.Object);
    }

    #region Nothing To Purge

    [Fact]
    public async Task Execute_WithNoExpiredOtps_ShouldNotCommit()
    {
        // Arrange
        _otpRepositoryMock.SetupCleanupExpiredOtps(0);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _otpRepositoryMock.Verify(x => x.CleanupExpiredOtpsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Purge

    [Fact]
    public async Task Execute_WithExpiredOtps_ShouldCommitOnceForTheBatch()
    {
        // Arrange
        _otpRepositoryMock.SetupCleanupExpiredOtps(3);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _otpRepositoryMock.Verify(x => x.CleanupExpiredOtpsAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Failure Tolerance

    [Fact]
    public async Task Execute_WhenTheRepositoryThrows_ShouldSwallowTheFailureAndNotCommit()
    {
        // Arrange
        _otpRepositoryMock
            .Setup(x => x.CleanupExpiredOtpsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("connection lost"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenTheCommitThrows_ShouldSwallowTheFailure()
    {
        // Arrange
        _otpRepositoryMock.SetupCleanupExpiredOtps(2);
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("commit failed"));

        // Act
        Func<Task> act = () => _job.Execute(_jobContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
