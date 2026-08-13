using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum;

/// <summary>
/// Unit tests for <see cref="AdminLinkLyricsAlbumHandler"/>.
/// </summary>
public class AdminLinkLyricsAlbumHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminLinkLyricsAlbumHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminLinkLyricsAlbumHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _albumRepositoryMock = MockAlbumRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminLinkLyricsAlbumHandler(
            _lyricsRepositoryMock.Object,
            _albumRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenAlbumIdProvided_ShouldLinkAlbum()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);

        var command = new AdminLinkLyricsAlbumCommand(lyrics.Id, album.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.AlbumId.Should().Be(album.Id);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAlbumIdIsNull_ShouldCallUnlinkAlbumNotLinkAlbum()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(CategoryId, Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new AdminLinkLyricsAlbumCommand(lyrics.Id, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.AlbumId.Should().BeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
        _albumRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenAlbumIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        Guid nonExistentAlbumId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentAlbumId);

        var command = new AdminLinkLyricsAlbumCommand(lyrics.Id, nonExistentAlbumId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        lyrics.AlbumId.Should().BeNull();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
