using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink;

/// <summary>
/// Unit tests for <see cref="AdminUpsertSingleStreamingLinkHandler"/>.
/// </summary>
public class AdminUpsertSingleStreamingLinkHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpsertSingleStreamingLinkHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminUpsertSingleStreamingLinkHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpsertSingleStreamingLinkHandler(
            _lyricsRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenStandaloneSingleAndNoExistingLink_ShouldCreateNewLink()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminUpsertSingleStreamingLinkCommand(
            lyrics.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/track/abc"
        );

        // Act
        AdminUpsertSingleStreamingLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StreamingLinkId.Should().NotBeEmpty();
        _streamingLinkRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenExistingLink_ShouldUpdateUrlInstead()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        StreamingLinkEntity existing = StreamingLinkFactory.CreateForLyrics(lyrics.Id, EnumStreamingPlatform.Tidal);
        _streamingLinkRepositoryMock.SetupGetByLyricsAndPlatformAsync(lyrics.Id, EnumStreamingPlatform.Tidal, existing);

        var command = new AdminUpsertSingleStreamingLinkCommand(
            lyrics.Id,
            EnumStreamingPlatform.Tidal,
            "https://listen.tidal.com/track/updated"
        );

        // Act
        AdminUpsertSingleStreamingLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StreamingLinkId.Should().Be(existing.Id);
        existing.Url.Should().Be("https://listen.tidal.com/track/updated");
        _streamingLinkRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(lyricsId);
        var command = new AdminUpsertSingleStreamingLinkCommand(
            lyricsId,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/track/abc"
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    /// <summary>
    /// The important business-rule test: a track that belongs to an album must get its
    /// streaming links through the album's own endpoint, not per-track — this call must be
    /// rejected with a conflict rather than silently creating a per-track link.
    /// </summary>
    [Fact]
    public async Task Handle_WhenLyricsBelongsToAlbum_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(CategoryId, Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var command = new AdminUpsertSingleStreamingLinkCommand(
            lyrics.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/track/abc"
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
