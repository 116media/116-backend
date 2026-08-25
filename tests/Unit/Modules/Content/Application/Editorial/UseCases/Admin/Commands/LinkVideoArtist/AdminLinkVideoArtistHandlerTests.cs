using _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.LinkVideoArtist;

/// <summary>
/// Unit tests for <see cref="AdminLinkVideoArtistHandler"/>.
/// </summary>
public class AdminLinkVideoArtistHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IArtistRepository> _artistRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminLinkVideoArtistHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminLinkVideoArtistHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _artistRepositoryMock = MockArtistRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminLinkVideoArtistHandler(
            _videoRepositoryMock.Object,
            _artistRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenArtistIdProvided_ShouldLinkArtist()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        ArtistEntity artist = ArtistFactory.Create();
        _artistRepositoryMock.SetupGetByIdOrThrow(artist);

        var command = new AdminLinkVideoArtistCommand(video.Id, artist.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ArtistId.Should().Be(artist.Id);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistIdIsNull_ShouldCallUnlinkArtistNotLinkArtist()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateForArtist(CategoryId, Guid.NewGuid());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        var command = new AdminLinkVideoArtistCommand(video.Id, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.ArtistId.Should().BeNull();
        _artistRepositoryMock.Verify(
            x => x.GetByIdOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenArtistIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        Guid nonExistentArtistId = Guid.NewGuid();
        _artistRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentArtistId);

        var command = new AdminLinkVideoArtistCommand(video.Id, nonExistentArtistId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        video.ArtistId.Should().BeNull();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
