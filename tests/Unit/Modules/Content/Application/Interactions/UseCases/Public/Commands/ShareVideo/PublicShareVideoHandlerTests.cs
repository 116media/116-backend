using _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;

/// <summary>
/// Unit tests for <see cref="PublicShareVideoHandler"/>.
/// </summary>
public class PublicShareVideoHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicShareVideoHandler _handler;

    public PublicShareVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicShareVideoHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenVideoExistsAndAnonymous_ShouldAddShareIncrementAndCommit()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var command = new PublicShareVideoCommand(VideoId: video.Id, UserId: null);

        // Act
        PublicShareVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyAddShareCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoExistsAndAuthenticated_ShouldAddShareWithUserId()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var command = new PublicShareVideoCommand(VideoId: video.Id, UserId: Guid.NewGuid());

        // Act
        PublicShareVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _videoRepositoryMock.VerifyAddShareCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(videoId);

        var command = new PublicShareVideoCommand(VideoId: videoId, UserId: null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
