using _116.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnbookmarkShortVideo;

/// <summary>
/// Unit tests for <see cref="PublicUnbookmarkShortVideoHandler"/>.
/// </summary>
public class PublicUnbookmarkShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnbookmarkShortVideoHandler _handler;

    public PublicUnbookmarkShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnbookmarkShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExistsAndBookmarked_ShouldRemoveBookmarkAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(userId, shortVideo.Id, result: true);

        var command = new PublicUnbookmarkShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _shortVideoRepositoryMock.VerifyRemoveBookmarkCalled(userId, shortVideo.Id);
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

        var command = new PublicUnbookmarkShortVideoCommand(ShortVideoId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBookmarkNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var userId = Guid.NewGuid();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasBookmarkedAsync(userId, shortVideo.Id, result: false);

        var command = new PublicUnbookmarkShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
