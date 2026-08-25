using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminDeleteShortVideoHandler"/>.
/// </summary>
public class AdminDeleteShortVideoHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteShortVideoHandler _handler;

    public AdminDeleteShortVideoHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        _handler = new AdminDeleteShortVideoHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasThumbnail_ShouldRaiseDeletionEventWithBothFileIds()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.CreateWithThumbnail();
        var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        ShortVideoDeletedEvent deletedEvent = shortVideo
            .DomainEvents.OfType<ShortVideoDeletedEvent>()
            .Should()
            .ContainSingle()
            .Subject;
        deletedEvent.VideoFileId.Should().Be(shortVideo.VideoFileId);
        deletedEvent.ThumbnailFileId.Should().Be(shortVideo.ThumbnailFileId);
        _shortVideoRepositoryMock.VerifyRemoveCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoHasNoThumbnail_ShouldRaiseDeletionEventWithVideoFileOnly()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        var command = new AdminDeleteShortVideoCommand(Id: shortVideo.Id.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        ShortVideoDeletedEvent deletedEvent = shortVideo
            .DomainEvents.OfType<ShortVideoDeletedEvent>()
            .Should()
            .ContainSingle()
            .Subject;
        deletedEvent.VideoFileId.Should().Be(shortVideo.VideoFileId);
        deletedEvent.ThumbnailFileId.Should().BeNull();
        _shortVideoRepositoryMock.VerifyRemoveCalled(shortVideo);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteShortVideoCommand(Id: nonExistentId.ToString());
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
