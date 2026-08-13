using _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink;

/// <summary>
/// Unit tests for <see cref="AdminRemoveAlbumStreamingLinkHandler"/>.
/// </summary>
public class AdminRemoveAlbumStreamingLinkHandlerTests
{
    private readonly Mock<IStreamingLinkRepository> _streamingLinkRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRemoveAlbumStreamingLinkHandler _handler;

    public AdminRemoveAlbumStreamingLinkHandlerTests()
    {
        _streamingLinkRepositoryMock = MockStreamingLinkRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRemoveAlbumStreamingLinkHandler(
            _streamingLinkRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenLinkExists_ShouldRemoveAndCommit()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        StreamingLinkEntity existing = StreamingLinkFactory.CreateForAlbum(albumId, EnumStreamingPlatform.Spotify);
        _streamingLinkRepositoryMock.SetupGetByAlbumAndPlatformAsync(albumId, EnumStreamingPlatform.Spotify, existing);
        var command = new AdminRemoveAlbumStreamingLinkCommand(albumId, EnumStreamingPlatform.Spotify);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _streamingLinkRepositoryMock.VerifyRemoveCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNoLinkExists_ShouldBeNoOpAndStillSucceed()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        var command = new AdminRemoveAlbumStreamingLinkCommand(albumId, EnumStreamingPlatform.Spotify);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _streamingLinkRepositoryMock.Verify(x => x.Remove(It.IsAny<StreamingLinkEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
