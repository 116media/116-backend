using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo;

/// <summary>
/// Unit tests for <see cref="AdminArchiveVideoHandler"/>.
/// </summary>
public class AdminArchiveVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminArchiveVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminArchiveVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminArchiveVideoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenVideoIsPublished_ShouldTransitionToArchived()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var command = new AdminArchiveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.Archived);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoIsPublished_ShouldRaiseVideoUnpublishedEvent()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminArchiveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video
            .DomainEvents.OfType<VideoUnpublishedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoUnpublishedEvent(VideoId: video.Id));
    }

    [Fact]
    public async Task Handle_WhenVideoIsNotPublished_ShouldArchiveWithoutUnpublishedEvent()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminArchiveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.Archived);
        video.DomainEvents.Should().BeEmpty();
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminArchiveVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoAlreadyArchived_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateArchived(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminArchiveVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        video.Status.Should().Be(EnumContentStatus.Archived);
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
