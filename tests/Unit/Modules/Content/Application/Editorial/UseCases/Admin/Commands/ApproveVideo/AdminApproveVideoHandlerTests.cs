using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveVideo;

/// <summary>
/// Unit tests for <see cref="AdminApproveVideoHandler"/>.
/// </summary>
public class AdminApproveVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminApproveVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminApproveVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminApproveVideoHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenVideoInPendingReview_ShouldApproveAndReturnSuccess()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingReview(CategoryId);
        var command = new AdminApproveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminApproveVideoResult result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new AdminApproveVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVideoAlreadyApproved_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApproved(CategoryId);
        var command = new AdminApproveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenVideoInWrongStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId); // Draft
        var command = new AdminApproveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
