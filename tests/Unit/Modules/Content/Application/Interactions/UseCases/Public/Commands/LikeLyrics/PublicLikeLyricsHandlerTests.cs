using _116.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics;

/// <summary>
/// Unit tests for <see cref="PublicLikeLyricsHandler"/>.
/// </summary>
public class PublicLikeLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicLikeLyricsHandler _handler;

    public PublicLikeLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicLikeLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsExistsAndNotLiked_ShouldAddLikeIncrementAndCommit()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(false);

        var command = new PublicLikeLyricsCommand(LyricsId: lyrics.Id, UserId: Guid.NewGuid());

        // Act
        PublicLikeLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lyrics.LikeCount.Should().Be(1);
        _lyricsRepositoryMock.VerifyAddLikeCalled();
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

        var command = new PublicLikeLyricsCommand(LyricsId: id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAlreadyLiked_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock.SetupHasLikedAsync(true);

        var command = new PublicLikeLyricsCommand(LyricsId: lyrics.Id, UserId: Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
