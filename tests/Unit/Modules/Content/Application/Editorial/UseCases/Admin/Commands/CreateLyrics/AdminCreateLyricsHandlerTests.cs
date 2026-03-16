using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminCreateLyricsHandler"/>.
/// </summary>
public class AdminCreateLyricsHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateLyricsHandler _handler;

    public AdminCreateLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminCreateLyricsHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }

    private static AdminCreateLyricsCommand BuildCommand(Guid? videoId = null, Guid? articleId = null) =>
        new(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: videoId,
            ArticleId: articleId
        );

    #region Success Cases

    [Fact]
    public async Task Handle_WhenStandaloneLyrics_ShouldCreateAndReturnLyrics()
    {
        // Arrange
        var command = BuildCommand();

        LyricsEntity? capturedEntity = null;
        _lyricsRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()))
            .Callback<LyricsEntity, CancellationToken>((e, _) => capturedEntity = e)
            .Returns(Task.CompletedTask);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedEntity!);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsForVideo_ShouldCreateAndReturnLyrics()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        var command = BuildCommand(videoId: videoId);

        LyricsEntity? capturedEntity = null;
        _lyricsRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()))
            .Callback<LyricsEntity, CancellationToken>((e, _) => capturedEntity = e)
            .Returns(Task.CompletedTask);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedEntity!);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsForArticle_ShouldCreateAndReturnLyrics()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        var command = BuildCommand(articleId: articleId);

        LyricsEntity? capturedEntity = null;
        _lyricsRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()))
            .Callback<LyricsEntity, CancellationToken>((e, _) => capturedEntity = e)
            .Returns(Task.CompletedTask);
        _lyricsRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedEntity!);

        // Act
        AdminCreateLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Lyrics.Should().NotBeNull();
        _lyricsRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenDuplicateSongTitleAndArtist_ShouldThrowConflictException()
    {
        // Arrange
        var command = BuildCommand();
        LyricsEntity existing = LyricsFactory.Create(command.SongTitle, command.ArtistName);
        _lyricsRepositoryMock.SetupGetBySongTitleAndArtist(command.SongTitle, command.ArtistName, existing);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
