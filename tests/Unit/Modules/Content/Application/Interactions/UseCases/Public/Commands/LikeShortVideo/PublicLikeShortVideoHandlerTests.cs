using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo;

/// <summary>
/// Unit tests for <see cref="PublicLikeShortVideoHandler"/>.
/// </summary>
public class PublicLikeShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicLikeShortVideoHandler _handler;

    public PublicLikeShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicLikeShortVideoHandler(
            _shortVideoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExistsAndNotLiked_ShouldAddLikeAndCommit()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var userId = Guid.NewGuid();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(userId, shortVideo.Id, result: false);

        var command = new PublicLikeShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _shortVideoRepositoryMock.VerifyAddLikeCalled();
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

        var command = new PublicLikeShortVideoCommand(ShortVideoId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyLiked_ShouldThrowConflictException()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var userId = Guid.NewGuid();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasLikedAsync(userId, shortVideo.Id, result: true);

        var command = new PublicLikeShortVideoCommand(ShortVideoId: shortVideo.Id, UserId: userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
