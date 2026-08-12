using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IShortVideoRepository"/>.
/// </summary>
public static class MockShortVideoRepository
{
    /// <summary>
    /// Creates a new mock instance of IShortVideoRepository with safe default setups.
    /// </summary>
    public static Mock<IShortVideoRepository> Create()
    {
        Mock<IShortVideoRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdOrThrow(
        this Mock<IShortVideoRepository> mock,
        ShortVideoEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdOrThrowNotFound(
        this Mock<IShortVideoRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Short video with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetByIdAsync(
        this Mock<IShortVideoRepository> mock,
        Guid id,
        ShortVideoEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetBySlug(
        this Mock<IShortVideoRepository> mock,
        string slug,
        ShortVideoEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetAllAsync(
        this Mock<IShortVideoRepository> mock,
        List<ShortVideoEntity> shortVideos,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((shortVideos, totalCount));
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetLikedAndBookmarkedIdsAsync(
        this Mock<IShortVideoRepository> mock,
        IReadOnlySet<Guid> liked,
        IReadOnlySet<Guid> bookmarked
    )
    {
        mock.Setup(x =>
                x.GetLikedAndBookmarkedIdsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((liked, bookmarked));
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupGetRandomizedFeedAsync(
        this Mock<IShortVideoRepository> mock,
        IReadOnlyList<ShortVideoEntity> items
    )
    {
        mock.Setup(x =>
                x.GetRandomizedFeedAsync(
                    It.IsAny<long>(),
                    It.IsAny<long?>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(items);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupCaptureRandomizedFeedArgs(
        this Mock<IShortVideoRepository> mock,
        IReadOnlyList<ShortVideoEntity> items,
        Action<long, long?, int> capture
    )
    {
        mock.Setup(x =>
                x.GetRandomizedFeedAsync(
                    It.IsAny<long>(),
                    It.IsAny<long?>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<long, long?, int, CancellationToken>((seed, sortKey, limit, _) => capture(seed, sortKey, limit))
            .ReturnsAsync(items);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<IShortVideoRepository> mock, ShortVideoEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IShortVideoRepository> mock, ShortVideoEntity shortVideo)
    {
        mock.Verify(x => x.Remove(shortVideo), Times.Once);
    }

    /// <summary>
    /// Answers the like-existence check for one user and short video pair only. Any other pair
    /// falls through to the default false, so a handler that asks on behalf of another user or
    /// about a different short video is not silently handed this answer.
    /// </summary>
    public static Mock<IShortVideoRepository> SetupHasLikedAsync(
        this Mock<IShortVideoRepository> mock,
        Guid userId,
        Guid shortVideoId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasLikedAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == shortVideoId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Answers the bookmark-existence check for one user and short video pair only. Any other pair
    /// falls through to the default false, so a handler that asks on behalf of another user or
    /// about a different short video is not silently handed this answer.
    /// </summary>
    public static Mock<IShortVideoRepository> SetupHasBookmarkedAsync(
        this Mock<IShortVideoRepository> mock,
        Guid userId,
        Guid shortVideoId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasBookmarkedAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == shortVideoId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static void VerifyAddLikeCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.AddLikeAsync(It.IsAny<ShortVideoLikeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveLikeCalled(this Mock<IShortVideoRepository> mock, Guid userId, Guid shortVideoId)
    {
        mock.Verify(x => x.RemoveLikeAsync(userId, shortVideoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddBookmarkCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(
            x => x.AddBookmarkAsync(It.IsAny<ShortVideoBookmarkEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemoveBookmarkCalled(this Mock<IShortVideoRepository> mock, Guid userId, Guid shortVideoId)
    {
        mock.Verify(x => x.RemoveBookmarkAsync(userId, shortVideoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddShareCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.AddShareAsync(It.IsAny<ShortVideoShareEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static Mock<IShortVideoRepository> SetupHasCountedViewSinceAsync(
        this Mock<IShortVideoRepository> mock,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasCountedViewSinceAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IShortVideoRepository> SetupCaptureViewEvent(
        this Mock<IShortVideoRepository> mock,
        Action<ShortVideoViewEventEntity> capture
    )
    {
        mock.Setup(x => x.AddViewEventAsync(It.IsAny<ShortVideoViewEventEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ShortVideoViewEventEntity, CancellationToken>((viewEvent, _) => capture(viewEvent))
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static void VerifyAddViewEventCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(
            x => x.AddViewEventAsync(It.IsAny<ShortVideoViewEventEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyUpdateNotCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<ShortVideoEntity>()), Times.Never);
    }

    public static void VerifyHasCountedViewSinceNotCalled(this Mock<IShortVideoRepository> mock)
    {
        mock.Verify(
            x =>
                x.HasCountedViewSinceAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IShortVideoRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ShortVideoEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ShortVideoEntity>(), 0));
        mock.Setup(x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x =>
                x.GetLikedAndBookmarkedIdsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(((IReadOnlySet<Guid>)new HashSet<Guid>(), (IReadOnlySet<Guid>)new HashSet<Guid>()));
        mock.Setup(x =>
                x.GetRandomizedFeedAsync(
                    It.IsAny<long>(),
                    It.IsAny<long?>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((IReadOnlyList<ShortVideoEntity>)new List<ShortVideoEntity>());
        mock.Setup(x => x.AddLikeAsync(It.IsAny<ShortVideoLikeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddBookmarkAsync(It.IsAny<ShortVideoBookmarkEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveBookmarkAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddShareAsync(It.IsAny<ShortVideoShareEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddViewEventAsync(It.IsAny<ShortVideoViewEventEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.HasCountedViewSinceAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        mock.Setup(x => x.PruneUncountedViewEventsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
    }
}
