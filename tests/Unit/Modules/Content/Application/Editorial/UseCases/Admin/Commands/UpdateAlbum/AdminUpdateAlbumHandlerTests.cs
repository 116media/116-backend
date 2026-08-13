using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Unit tests for <see cref="AdminUpdateAlbumHandler"/>.
/// </summary>
public class AdminUpdateAlbumHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateAlbumHandler _handler;

    public AdminUpdateAlbumHandlerTests()
    {
        _albumRepositoryMock = MockAlbumRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateAlbumHandler(
            _albumRepositoryMock.Object,
            _unitOfWorkMock.Object,
            fileRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldUpdateAndReturnAlbum()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        var command = new AdminUpdateAlbumCommand(
            album.Id,
            "Updated Name",
            2001,
            "Updated Label",
            EnumReleaseType.Album
        );

        // Act
        AdminUpdateAlbumResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        album.Name.Should().Be("Updated Name");
        album.ReleaseYear.Should().Be(2001);
        album.Label.Should().Be("Updated Label");
        album.ReleaseType.Should().Be(EnumReleaseType.Album);
        result.Album.Name.Should().Be("Updated Name");
        result.Album.ReleaseYear.Should().Be(2001);
        result.Album.Label.Should().Be("Updated Label");
        _albumRepositoryMock.VerifyUpdateCalled(album);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldPreserveExistingCoverImageFileId()
    {
        // Arrange
        Guid coverImageFileId = Guid.NewGuid();
        AlbumEntity album = AlbumFactory.CreateWithCoverImageFileId(coverImageFileId);
        _albumRepositoryMock.SetupGetByIdOrThrow(album);
        var command = new AdminUpdateAlbumCommand(album.Id, "Updated Name", null, null, EnumReleaseType.Album);

        // Act
        AdminUpdateAlbumResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Album.CoverImageUrl.Should().BeNull();
        album.CoverImageFileId.Should().Be(coverImageFileId);
        album.Name.Should().Be("Updated Name");
        _albumRepositoryMock.VerifyUpdateCalled(album);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAlbumNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _albumRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new AdminUpdateAlbumCommand(nonExistentId, "Name", null, null, EnumReleaseType.Album);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
