using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Constants;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Events;
using _116.Core.Infrastructure.BackgroundJobs;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Quartz;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.BackgroundJobs;

/// <summary>
/// Unit tests for <see cref="UnclaimedFileReaperJob" />: uploads no referencing write ever
/// claimed are soft-deleted so their remote assets get cleaned, and everything else is left alone.
/// </summary>
public class UnclaimedFileReaperJobTests
{
    private readonly Mock<IFileRepository> _fileRepositoryMock = MockFileRepository.Create();
    private readonly Mock<ICoreUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IJobExecutionContext> _jobContextMock = new();
    private readonly UnclaimedFileReaperJob _job;

    public UnclaimedFileReaperJobTests()
    {
        _jobContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var services = new ServiceCollection();
        services.AddScoped(_ => _fileRepositoryMock.Object);
        services.AddScoped(_ => _unitOfWorkMock.Object);
        services.AddSingleton(TimeProvider.System);

        _job = new UnclaimedFileReaperJob(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            NullLogger<UnclaimedFileReaperJob>.Instance
        );
    }

    /// <summary>
    /// Points the reaper's scan at the supplied uploads.
    /// </summary>
    /// <param name="abandoned">The uploads the scan should return.</param>
    private void SetupAbandoned(params FileEntity[] abandoned)
    {
        _fileRepositoryMock
            .Setup(x => x.GetUnclaimedBeforeAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(abandoned);
    }

    [Fact]
    public async Task Execute_WithNothingAbandoned_ShouldNotSave()
    {
        // Arrange
        SetupAbandoned();

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldSoftDeleteTheAbandonedUploadAndRaiseItsCleanupEvent()
    {
        // Arrange
        FileEntity abandoned = FileFactory.CreateImage();
        SetupAbandoned(abandoned);

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        abandoned.IsDeleted.Should().BeTrue();
        abandoned.DomainEvents.OfType<FileSoftDeletedEvent>().Should().ContainSingle();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldOnlyScanBeyondTheGracePeriod()
    {
        // Arrange
        DateTime before = DateTime.UtcNow - CoreConstants.UnclaimedFileGracePeriod;
        SetupAbandoned();

        // Act
        await _job.Execute(_jobContextMock.Object);

        // Assert
        _fileRepositoryMock.Verify(
            x =>
                x.GetUnclaimedBeforeAsync(
                    It.Is<DateTime>(cutoff => cutoff <= before.AddSeconds(1)),
                    CoreConstants.UnclaimedFileReapBatchSize,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
