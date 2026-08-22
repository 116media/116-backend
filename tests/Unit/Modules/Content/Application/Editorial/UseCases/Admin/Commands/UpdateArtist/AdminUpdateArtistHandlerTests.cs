using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArtistHandler"/>.
/// </summary>
public class AdminUpdateArtistHandlerTests
{
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminUpdateArtistHandler _handler;

    public AdminUpdateArtistHandlerTests()
    {
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        Mock<IFileRepository> fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminUpdateArtistHandler(
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object,
            fileRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldUpdateAndReturnArtist()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminUpdateArtistCommand(artist.Id, "Updated Name", "Updated Bio", null, null, null, null);

        // Act
        AdminUpdateArtistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist.Name.Should().Be("Updated Name");
        artist.Bio.Should().Be("Updated Bio");
        artist.RealName.Should().BeNull();
        artist.Aliases.Should().BeEmpty();
        artist.Birthdate.Should().BeNull();
        artist.Hometown.Should().BeNull();
        artist.NameFolded.Should().Be("UPDATED NAME");
        artist.InitialLetter.Should().Be("U");
        result.Artist.Name.Should().Be("Updated Name");
        result.Artist.Bio.Should().Be("Updated Bio");
        _artistRepositoryMock.VerifyUpdateCalled(artist);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_ShouldNotChangeSlug()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        string originalSlug = artist.Slug;
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);
        var command = new AdminUpdateArtistCommand(artist.Id, "Updated Name", null, null, null, null, null);

        // Act
        AdminUpdateArtistResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        artist.Slug.Should().Be(originalSlug);
        result.Artist.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public async Task Handle_WhenArtistNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);
        var command = new AdminUpdateArtistCommand(nonExistentId, "Name", null, null, null, null, null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
