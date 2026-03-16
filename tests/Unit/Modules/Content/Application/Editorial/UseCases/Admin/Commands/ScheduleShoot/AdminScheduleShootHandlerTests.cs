using _116.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ScheduleShoot;

/// <summary>
/// Unit tests for <see cref="AdminScheduleShootHandler"/>.
/// </summary>
public class AdminScheduleShootHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminScheduleShootHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminScheduleShootHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminScheduleShootHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldScheduleShootAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        DateTimeOffset scheduledAt = DateTimeOffset.UtcNow.AddDays(7);
        var command = new AdminScheduleShootCommand(VideoId: video.Id.ToString(), ShootingScheduledAt: scheduledAt);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminScheduleShootResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminScheduleShootCommand(
            VideoId: nonExistentId.ToString(),
            ShootingScheduledAt: DateTimeOffset.UtcNow.AddDays(7)
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
