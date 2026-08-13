using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;
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

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Unit tests for <see cref="PublicRateVideoHandler"/>.
/// </summary>
public class PublicRateVideoHandlerTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRateVideoHandler _handler;

    public PublicRateVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRateVideoHandler(_videoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNewRating_ShouldAddRatingAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetRatingNotFound(userId, video.Id);

        var command = new PublicRateVideoCommand(VideoId: video.Id, UserId: userId, Stars: 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _videoRepositoryMock.VerifyAddRatingCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenExistingRating_ShouldUpdateRatingAndCommit()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        VideoRatingEntity existingRating = VideoRatingFactory.Create(video.Id, userId, stars: 3);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetRatingAsync(existingRating);

        var command = new PublicRateVideoCommand(VideoId: video.Id, UserId: userId, Stars: 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingRating.Stars.Should().Be(5);
        _videoRepositoryMock.VerifyUpdateRatingCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(videoId);

        var command = new PublicRateVideoCommand(VideoId: videoId, UserId: Guid.NewGuid(), Stars: 5);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
