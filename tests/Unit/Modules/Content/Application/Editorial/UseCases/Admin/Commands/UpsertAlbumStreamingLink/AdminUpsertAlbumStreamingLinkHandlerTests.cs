using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpsertAlbumStreamingLink;

/// <summary>
/// Unit tests for <see cref="AdminUpsertAlbumStreamingLinkHandler"/>.
/// </summary>
public class AdminUpsertAlbumStreamingLinkHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpsertAlbumStreamingLinkHandler _handler;

    public AdminUpsertAlbumStreamingLinkHandlerTests()
    {
        _albumRepositoryMock = MockAlbumRepository.Create();
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminUpsertAlbumStreamingLinkHandler(
            _albumRepositoryMock.Object,
            _streamingLinkRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNoExistingLink_ShouldCreateNewLink()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        var command = new AdminUpsertAlbumStreamingLinkCommand(
            album.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/album/abc"
        );

        // Act
        AdminUpsertAlbumStreamingLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StreamingLinkId.Should().NotBeEmpty();
        _streamingLinkRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<StreamingLinkEntity>(link =>
                        link.Id == result.StreamingLinkId
                        && link.AlbumId == album.Id
                        && link.Platform == EnumStreamingPlatform.Spotify
                        && link.Url == "https://open.spotify.com/album/abc"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenExistingLink_ShouldUpdateUrlInstead()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        StreamingLinkEntity existing = StreamingLinkFactory.CreateForAlbum(album.Id, EnumStreamingPlatform.Spotify);
        _streamingLinkRepositoryMock.SetupGetByAlbumAndPlatformAsync(album.Id, EnumStreamingPlatform.Spotify, existing);

        var command = new AdminUpsertAlbumStreamingLinkCommand(
            album.Id,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/album/updated"
        );

        // Act
        AdminUpsertAlbumStreamingLinkResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.StreamingLinkId.Should().Be(existing.Id);
        existing.Url.Should().Be("https://open.spotify.com/album/updated");
        existing.Platform.Should().Be(EnumStreamingPlatform.Spotify);
        _streamingLinkRepositoryMock.VerifyUpdateCalled(existing);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAlbumNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(albumId);
        var command = new AdminUpsertAlbumStreamingLinkCommand(
            albumId,
            EnumStreamingPlatform.Spotify,
            "https://open.spotify.com/album/abc"
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
