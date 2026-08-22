using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Unit tests for <see cref="AdminCreateAlbumHandler"/>.
/// </summary>
public class AdminCreateAlbumHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminCreateAlbumHandler _handler;

    public AdminCreateAlbumHandlerTests()
    {
        _albumRepositoryMock = MockAlbumRepository.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminCreateAlbumHandler(
            _albumRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            fileRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenValidStandaloneCommand_ShouldCreateAndReturnAlbum()
    {
        // Arrange
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            null,
            TestConstants.Album.ValidReleaseYear,
            TestConstants.Album.ValidLabel,
            EnumReleaseType.Album
        );

        // Act
        AdminCreateAlbumResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Album.Name.Should().Be(command.Name);
        result.Album.ArtistId.Should().BeNull();
        _albumRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistIdProvided_ShouldVerifyArtistExistsAndLinkIt()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            artist.Id,
            null,
            null,
            EnumReleaseType.Album
        );

        // Act
        AdminCreateAlbumResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Album.ArtistId.Should().Be(artist.Id);
        _artistRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(artist.Id, It.IsAny<CancellationToken>()), Times.Once);
        _albumRepositoryMock.VerifyAddCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenArtistIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentArtistId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentArtistId);
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            nonExistentArtistId,
            null,
            null,
            EnumReleaseType.Album
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenArtistIdDoesNotExist_ShouldNotAddOrCommit()
    {
        // Arrange
        Guid nonExistentArtistId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentArtistId);
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            nonExistentArtistId,
            null,
            null,
            EnumReleaseType.Album
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _albumRepositoryMock.VerifyAddNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
