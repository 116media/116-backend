using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Unit tests for <see cref="AdminDeleteLyricsHandler"/>.
/// </summary>
public class AdminDeleteLyricsHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDeleteLyricsHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminDeleteLyricsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDeleteLyricsHandler(
            _lyricsRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
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
        AdminDeleteLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _lyricsRepositoryMock.VerifyRemoveCalled(lyrics);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenLyricsLinkedToVideo_ShouldUnmarkVideoAndDelete()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, Guid.NewGuid());
        var command = new AdminDeleteLyricsCommand(Id: lyrics.Id.ToString());

        VideoEntity video = VideoFactory.Create(Guid.NewGuid());
        video.MarkHasLyrics();

        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(lyrics.VideoId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        AdminDeleteLyricsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        video.HasLyrics.Should().BeFalse();
        _videoRepositoryMock.VerifyUpdateCalled();
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
    }

    #endregion
}
