using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Unit tests for <see cref="AdminSubmitVideoHandler"/>.
/// </summary>
public class AdminSubmitVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminSubmitVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminSubmitVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminSubmitVideoHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenFreeVideoInDraft_ShouldTransitionToPendingReview()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminSubmitVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPaidVideoInDraft_ShouldTransitionToPendingPayment()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, Guid.NewGuid(), Guid.NewGuid());
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminSubmitVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminSubmitVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFreeVideoAlreadyPendingReview_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingReview(CategoryId);
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPaidVideoAlreadyPendingPayment_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingPayment(CategoryId, Guid.NewGuid(), Guid.NewGuid());
        var command = new AdminSubmitVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
