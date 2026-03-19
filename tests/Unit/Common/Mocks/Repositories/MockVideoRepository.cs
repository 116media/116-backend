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
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((videos, totalCount));
        return mock;
    }

    public static Mock<IVideoRepository> SetupGetFeaturedAsync(
        this Mock<IVideoRepository> mock,
        IReadOnlyList<VideoEntity> videos
    )
    {
        mock.Setup(x => x.GetFeaturedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(videos);
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

    public static void VerifyUpdateCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<VideoEntity>()), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IVideoRepository> mock, VideoEntity video)
    {
        mock.Verify(x => x.Remove(video), Times.Once);
    }

    public static void VerifyAddTagCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveTagCalled(this Mock<IVideoRepository> mock)
    {
        mock.Verify(x => x.RemoveTag(It.IsAny<VideoTagEntity>()), Times.Once);
    }

    public static Mock<IVideoRepository> SetupGetRatingAsync(
        this Mock<IVideoRepository> mock,
        VideoRatingEntity? rating
    )
    {
        mock.Setup(x => x.GetRatingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rating);
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

    private static void SetupDefaults(Mock<IVideoRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<VideoEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.AddTagAsync(It.IsAny<VideoTagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByOrderItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoEntity?)null);
        mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoEntity?)null);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoEntity?)null);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<EnumContentStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<VideoEntity>(), 0));
        mock.Setup(x => x.GetFeaturedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<VideoEntity>());
        mock.Setup(x => x.GetTagsByVideoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VideoTagEntity>());
        mock.Setup(x => x.GetRatingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VideoRatingEntity?)null);
        mock.Setup(x => x.GetAllRatingsForVideoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VideoRatingEntity>());
        mock.Setup(x => x.AddRatingAsync(It.IsAny<VideoRatingEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddShareAsync(It.IsAny<VideoShareEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
