using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;

/// <summary>
/// Unit tests for <see cref="AdminArchiveLyricsHandler"/>.
/// </summary>
public class AdminArchiveLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminArchiveLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminArchiveLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminArchiveLyricsHandler(
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenLyricsIsPublished_ShouldTransitionToArchived()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        var command = new AdminArchiveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Archived);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsIsDraft_ShouldTransitionToArchived()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminArchiveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Archived);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminArchiveLyricsCommand(Id: nonExistentId.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsAlreadyArchived_ShouldThrowConflictException()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateArchived(CategoryId);
        var command = new AdminArchiveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        lyrics.Status.Should().Be(EnumContentStatus.Archived);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
