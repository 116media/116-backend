using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;

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
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsIsApproved_ShouldTransitionToArchived()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        lyrics.MarkPendingReview();
        lyrics.Approve();
        var command = new AdminArchiveLyricsCommand(Id: lyrics.Id.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        lyrics.Status.Should().Be(EnumContentStatus.Archived);
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
