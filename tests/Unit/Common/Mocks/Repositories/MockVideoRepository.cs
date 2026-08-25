using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IVideoRepository"/>.
/// </summary>
public static class MockVideoRepository
{
    /// <summary>
    /// Creates a new mock instance of IVideoRepository with safe default setups.
    /// </summary>
    public static Mock<IVideoRepository> Create()
    {
        Mock<IVideoRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetByIdOrThrow(this Mock<IVideoRepository> mock, VideoEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetByIdOrThrowNotFound(this Mock<IVideoRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Video with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetByIdAsync(
        this Mock<IVideoRepository> mock,
        Guid id,
        VideoEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetByOrderItemIdAsync(
        this Mock<IVideoRepository> mock,
        Guid orderItemId,
        VideoEntity? entity
    )
    {
        mock.Setup(x => x.GetByOrderItemIdAsync(orderItemId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetBySlug(
        this Mock<IVideoRepository> mock,
        string slug,
        VideoEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetAllAsync(
        this Mock<IVideoRepository> mock,
        List<VideoEntity> videos,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<EnumContentStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((videos, totalCount));
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetPublishedByArtist(
        this Mock<IVideoRepository> mock,
        Guid artistId,
        List<VideoEntity> videos,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetPublishedByArtistAsync(artistId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((videos, totalCount));
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetActiveAsync(this Mock<IVideoRepository> mock, List<VideoEntity> videos)
    {
        mock.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(videos);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetPromotedAsync(
        this Mock<IVideoRepository> mock,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x => x.GetPromotedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(videos);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetPopularVideosAsync(
        this Mock<IVideoRepository> mock,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x =>
                x.GetPopularVideosAsync(
                    It.IsAny<int>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(videos);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetTagsByVideoId(
        this Mock<IVideoRepository> mock,
        Guid videoId,
        IReadOnlyList<VideoTagEntity> tags
    )
    {
        mock.Setup(x => x.GetTagsByVideoIdAsync(videoId, It.IsAny<CancellationToken>())).ReturnsAsync(tags);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<VideoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<IVideoRepository> mock, VideoEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IVideoRepository> mock, VideoEntity video)
    {
        mock.Verify(x => x.Remove(video), Times.Once);
    }

    public static void VerifyAddTagCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    public static void VerifyRemoveTagCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.RemoveTag(It.IsAny<VideoTagEntity>()), Times.Once);
    }

    /// <summary>
    /// Sets up the rating lookup to answer only for the rating's own user and video ids, so a
    /// handler that asks on behalf of another user or video is not silently handed this rating.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="rating">The rating returned for its own user and video identifiers.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IVideoRepository> SetupGetRatingAsync(this Mock<IVideoRepository> mock, VideoRatingEntity rating)
    {
        Guid ratedByUserId = rating.UserId;
        Guid ratedVideoId = rating.VideoId;
        mock.Setup(x =>
                x.GetRatingAsync(
                    It.Is<Guid>(id => id == ratedByUserId),
                    It.Is<Guid>(id => id == ratedVideoId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(rating);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for the given user and video pair, so the "not yet rated" branch is reached
    /// for the identifiers the test names rather than for every pair the handler could ask about.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="userId">The rating author identifier that must resolve to nothing.</param>
    /// <param name="videoId">The video identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IVideoRepository> SetupGetRatingNotFound(
        this Mock<IVideoRepository> mock,
        Guid userId,
        Guid videoId
    )
    {
        mock.Setup(x => x.GetRatingAsync(userId, videoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoRatingEntity?)null);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetAllRatingsForVideoAsync(
        this Mock<IVideoRepository> mock,
        List<VideoRatingEntity> ratings
    )
    {
        mock.Setup(x => x.GetAllRatingsForVideoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ratings);
        return mock;
    }

    public static void VerifyAddRatingCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.AddRatingAsync(It.IsAny<VideoRatingEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateRatingCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.UpdateRating(It.IsAny<VideoRatingEntity>()), Times.Once);
    }

    public static void VerifyAddShareCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.AddShareAsync(It.IsAny<VideoShareEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IVideoRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<VideoEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<EnumContentStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<VideoEntity>(), 0));
        mock.Setup(x => x.GetPromotedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VideoEntity>());
        mock.Setup(x => x.GetTagsByVideoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VideoTagEntity>());
        mock.Setup(x => x.GetAllRatingsForVideoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VideoRatingEntity>());
        mock.Setup(x => x.AddRatingAsync(It.IsAny<VideoRatingEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddShareAsync(It.IsAny<VideoShareEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetActivePromotedBySpotAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VideoEntity>());
        mock.Setup(x =>
                x.GetFreeVideosAsync(It.IsAny<int>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<VideoEntity>());
        mock.Setup(x =>
                x.GetLatestPublishedByCategoryAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<VideoEntity>());
        mock.Setup(x => x.CountPublishedByCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        mock.Setup(x =>
                x.GetPublishedByArtistAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<VideoEntity>(), 0));
    }

    public static Mock<IVideoRepository> SetupGetLatestPublishedByCategory(
        this Mock<IVideoRepository> mock,
        Guid categoryId,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x => x.GetLatestPublishedByCategoryAsync(categoryId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos);
        return mock;
    }

    public static Mock<IVideoRepository> SetupCountPublishedByCategory(
        this Mock<IVideoRepository> mock,
        Guid categoryId,
        int count
    )
    {
        mock.Setup(x => x.CountPublishedByCategoryAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetActivePromotedBySpot(
        this Mock<IVideoRepository> mock,
        int spotPriority,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x => x.GetActivePromotedBySpotAsync(spotPriority, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videos);
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetFreeVideos(
        this Mock<IVideoRepository> mock,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x =>
                x.GetFreeVideosAsync(It.IsAny<int>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(videos);
        return mock;
    }
}
