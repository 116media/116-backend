using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Unit tests for <see cref="AdminResolveSingleStreamingLinksHandler"/>.
/// </summary>
public class AdminResolveSingleStreamingLinksHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IStreamingLinkResolutionService> _resolutionServiceMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminResolveSingleStreamingLinksHandler _handler;

    private const string SourceUrl = "https://open.spotify.com/track/xyz789";

    public AdminResolveSingleStreamingLinksHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _resolutionServiceMock = new Mock<IStreamingLinkResolutionService>();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminResolveSingleStreamingLinksHandler(
            _lyricsRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            _resolutionServiceMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WithStandaloneSingle_ShouldUpsertResolvedPlatforms()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<EnumStreamingPlatform, string>
                {
                    [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/1",
                    [EnumStreamingPlatform.AppleMusic] = "https://music.apple.com/track/2",
                }
            );

        var command = new AdminResolveSingleStreamingLinksCommand(lyrics.Id, SourceUrl);

        // Act
        AdminResolveSingleStreamingLinksResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Resolved.Should().Equal(EnumStreamingPlatform.Spotify, EnumStreamingPlatform.AppleMusic);
        _streamingLinkRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenSongBelongsToAlbum_ShouldRejectBeforeCallingTheProvider()
    {
        // Arrange — the album's links are the release's links; same rule as the manual upsert.
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(Guid.NewGuid(), Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new AdminResolveSingleStreamingLinksCommand(lyrics.Id, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _resolutionServiceMock.Verify(
            x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNothingResolves_ShouldThrowNotFound()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<EnumStreamingPlatform, string>());

        var command = new AdminResolveSingleStreamingLinksCommand(lyrics.Id, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenPlatformAlreadyCurated_ShouldReplaceInsteadOfDuplicating()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        StreamingLinkEntity existing = StreamingLinkEntity.ForSingle(
            Guid.NewGuid(),
            lyrics.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/track/old"
        );
        _streamingLinkRepositoryMock.SetupGetByLyricsAndPlatformAsync(
            lyrics.Id,
            EnumStreamingPlatform.Spotify,
            existing
        );
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<EnumStreamingPlatform, string>
                {
                    [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/track/new",
                }
            );

        var command = new AdminResolveSingleStreamingLinksCommand(lyrics.Id, SourceUrl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existing.Url.Should().Be("https://open.spotify.com/track/new");
        _streamingLinkRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
