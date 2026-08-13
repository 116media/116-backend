using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist;

/// <summary>
/// Unit tests for <see cref="AdminLinkLyricsArtistHandler"/>.
/// </summary>
public class AdminLinkLyricsArtistHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminLinkLyricsArtistHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminLinkLyricsArtistHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminLinkLyricsArtistHandler(
            _lyricsRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenArtistIdProvided_ShouldLinkArtist()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        var command = new AdminLinkLyricsArtistCommand(lyrics.Id, artist.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.ArtistId.Should().Be(artist.Id);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistIdIsNull_ShouldCallUnlinkArtistNotLinkArtist()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForArtist(CategoryId, Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new AdminLinkLyricsArtistCommand(lyrics.Id, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.ArtistId.Should().BeNull();
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
        _artistRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenArtistIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        Guid nonExistentArtistId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentArtistId);

        var command = new AdminLinkLyricsArtistCommand(lyrics.Id, nonExistentArtistId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        lyrics.ArtistId.Should().BeNull();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        var command = new AdminLinkLyricsArtistCommand(nonExistentId, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
