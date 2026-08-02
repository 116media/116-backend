using _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics;

/// <summary>
/// Unit tests for <see cref="PublicUnlikeLyricsHandler"/>.
/// </summary>
public class PublicUnlikeLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicUnlikeLyricsHandler _handler;

    public PublicUnlikeLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicUnlikeLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsExistsAndLiked_ShouldRemoveLikeDecrementAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        lyrics.IncrementLikeCount();

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        var command = new PublicUnlikeLyricsCommand(LyricsId: lyrics.Id, UserId: userId);

        // Act
        PublicUnlikeLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lyrics.LikeCount.Should().Be(0);
        _lyricsRepositoryMock.VerifyRemoveLikeCalled(userId, lyrics.Id);
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicUnlikeLyricsCommand(LyricsId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLikeNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(false);

        var command = new PublicUnlikeLyricsCommand(LyricsId: lyrics.Id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion
}
