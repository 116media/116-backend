using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;
using _116.Content.Application.Shared.Exceptions;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Unit tests for <see cref="AdminResolveAlbumStreamingLinksHandler"/>.
/// </summary>
public class AdminResolveAlbumStreamingLinksHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IStreamingLinkResolutionService> _resolutionServiceMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminResolveAlbumStreamingLinksHandler _handler;

    private const string SourceUrl = "https://open.spotify.com/album/abc123";

    public AdminResolveAlbumStreamingLinksHandlerTests()
    {
        _albumRepositoryMock = MockAlbumRepository.Create();
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _resolutionServiceMock = new Mock<IStreamingLinkResolutionService>();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminResolveAlbumStreamingLinksHandler(
            _albumRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            _resolutionServiceMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private void SetupResolution(IReadOnlyDictionary<EnumStreamingPlatform, string> result)
    {
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    [Fact]
    public async Task Handle_WithResolvedPlatforms_ShouldUpsertEachAndCommitOnce()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        SetupResolution(
            new Dictionary<EnumStreamingPlatform, string>
            {
                [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/1",
                [EnumStreamingPlatform.Deezer] = "https://www.deezer.com/album/5",
            }
        );

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        AdminResolveAlbumStreamingLinksResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert — two inserts, one commit, and the report splits resolved from unresolved.
        result.Resolved.Should().Equal(EnumStreamingPlatform.Spotify, EnumStreamingPlatform.Deezer);
        result
            .Unresolved.Should()
            .Equal(EnumStreamingPlatform.AppleMusic, EnumStreamingPlatform.YoutubeMusic, EnumStreamingPlatform.Tidal);
        _streamingLinkRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenPlatformAlreadyCurated_ShouldReplaceUrlInsteadOfDuplicating()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        StreamingLinkEntity existing = StreamingLinkEntity.ForAlbum(
            Guid.NewGuid(),
            album.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/album/old"
        );
        _streamingLinkRepositoryMock.SetupGetByAlbumAndPlatformAsync(album.Id, EnumStreamingPlatform.Spotify, existing);
        SetupResolution(
            new Dictionary<EnumStreamingPlatform, string>
            {
                [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/new",
            }
        );

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existing.Url.Should().Be("https://open.spotify.com/album/new");
        _streamingLinkRepositoryMock.Verify(x => x.Update(existing), Times.Once);
        _streamingLinkRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<StreamingLinkEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldNeverRemoveRowsForUnresolvedPlatforms()
    {
        // Arrange — a hand-curated Tidal row must survive a resolution that had no Tidal link.
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        SetupResolution(
            new Dictionary<EnumStreamingPlatform, string>
            {
                [EnumStreamingPlatform.Spotify] = "https://open.spotify.com/album/1",
            }
        );

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _streamingLinkRepositoryMock.Verify(x => x.Remove(It.IsAny<StreamingLinkEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNothingResolves_ShouldThrowNotFoundAndStoreNothing()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        SetupResolution(new Dictionary<EnumStreamingPlatform, string>());

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenProviderFails_ShouldPropagateResolutionException()
    {
        // Arrange — the command handler no longer maps the failure; the global pipeline does
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StreamingLinkResolutionException("down"));

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenProviderRateLimits_ShouldPropagateResolutionException()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        _resolutionServiceMock
            .Setup(x => x.ResolveAsync(SourceUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StreamingLinkResolutionException("slow down", isRateLimited: true));

        var command = new AdminResolveAlbumStreamingLinksCommand(album.Id, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<StreamingLinkResolutionException>();
    }

    [Fact]
    public async Task Handle_WhenAlbumDoesNotExist_ShouldThrowBeforeCallingTheProvider()
    {
        // Arrange
        var missingAlbumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(missingAlbumId);

        var command = new AdminResolveAlbumStreamingLinksCommand(missingAlbumId, SourceUrl);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert — no provider quota is spent on an album that does not exist.
        await act.Should().ThrowAsync<NotFoundException>();
        _resolutionServiceMock.Verify(
            x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
