using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
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
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Unit tests for <see cref="AdminDeleteLyricsHandler"/>.
/// </summary>
public class AdminDeleteLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminDeleteLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteLyricsHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenStandaloneLyrics_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var command = new AdminDeleteLyricsCommand(Id: lyrics.Id.ToString());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.VerifyRemoveCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToVideo_ShouldDeleteWithoutTouchingTheVideo()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, Guid.NewGuid());
        var command = new AdminDeleteLyricsCommand(Id: lyrics.Id.ToString());

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _lyricsRepositoryMock.VerifyRemoveCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminDeleteLyricsCommand(Id: nonExistentId.ToString());
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _lyricsRepositoryMock.Verify(x => x.Remove(It.IsAny<LyricsEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
