using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;

/// <summary>
/// Unit tests for <see cref="PublicUnlikeShortVideoHandler"/>.
/// </summary>
public class PublicUnlikeShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnlikeShortVideoHandler _handler;

    public PublicUnlikeShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnlikeShortVideoHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExistsAndLiked_ShouldRemoveLikeDecrementAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(true);

        var command = new PublicUnlikeShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: userId);

        // Act
        PublicUnlikeShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyRemoveLikeCalled(userId, shortVideo.Id);
        _shortVideoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicUnlikeShortVideoCommand(ShortVideoId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLikeNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(false);

        var command = new PublicUnlikeShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
