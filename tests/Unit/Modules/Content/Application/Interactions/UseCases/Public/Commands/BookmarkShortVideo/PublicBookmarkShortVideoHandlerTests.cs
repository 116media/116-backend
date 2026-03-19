using _116.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.BookmarkShortVideo;

/// <summary>
/// Unit tests for <see cref="PublicBookmarkShortVideoHandler"/>.
/// </summary>
public class PublicBookmarkShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicBookmarkShortVideoHandler _handler;

    public PublicBookmarkShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicBookmarkShortVideoHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExistsAndNotBookmarked_ShouldAddBookmarkIncrementAndCommit()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(false);

        var command = new PublicBookmarkShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: Guid.NewGuid());

        // Act
        PublicBookmarkShortVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyAddBookmarkCalled();
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

        var command = new PublicBookmarkShortVideoCommand(ShortVideoId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyBookmarked_ShouldThrowConflictException()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(true);

        var command = new PublicBookmarkShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
