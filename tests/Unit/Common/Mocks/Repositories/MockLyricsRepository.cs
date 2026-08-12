using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ILyricsRepository"/>.
/// </summary>
public static class MockLyricsRepository
{
    /// <summary>
    /// Creates a new mock instance of ILyricsRepository with safe default setups.
    /// </summary>
    public static Mock<ILyricsRepository> Create()
    {
        Mock<ILyricsRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdOrThrow(this Mock<ILyricsRepository> mock, LyricsEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdOrThrowNotFound(this Mock<ILyricsRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Lyrics with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByIdAsync(
        this Mock<ILyricsRepository> mock,
        Guid id,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetBySlug(
        this Mock<ILyricsRepository> mock,
        string slug,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetAllAsync(
        this Mock<ILyricsRepository> mock,
        List<LyricsEntity> lyrics,
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
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((lyrics, totalCount));
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the repository was handed exactly the expected entity once,
    /// so updating a different instance than the one looked up fails the test.
    /// </summary>
    public static void VerifyUpdateCalled(this Mock<ILyricsRepository> mock, LyricsEntity expected)
    {
        mock.Verify(x => x.Update(expected), Times.Once);
    }

    public static Mock<ILyricsRepository> SetupGetPublishedByArtist(
        this Mock<ILyricsRepository> mock,
        Guid artistId,
        List<LyricsEntity> lyrics,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetPublishedByArtistAsync(artistId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((lyrics, totalCount));
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByVideoIdAsync(
        this Mock<ILyricsRepository> mock,
        Guid videoId,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetByVideoIdAsync(videoId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static void VerifyRemoveCalled(this Mock<ILyricsRepository> mock, LyricsEntity lyrics)
    {
        mock.Verify(x => x.Remove(lyrics), Times.Once);
    }

    public static Mock<ILyricsRepository> SetupGetPublishedByAlbumAsync(
        this Mock<ILyricsRepository> mock,
        Guid albumId,
        Guid excludeLyricsId,
        List<LyricsEntity> lyrics
    )
    {
        mock.Setup(x => x.GetPublishedByAlbumAsync(albumId, excludeLyricsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lyrics);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetByOrderItemIdAsync(
        this Mock<ILyricsRepository> mock,
        Guid orderItemId,
        LyricsEntity? entity
    )
    {
        mock.Setup(x => x.GetByOrderItemIdAsync(orderItemId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    /// <summary>
    /// Answers the like-existence check for one user and lyrics pair only. Any other pair falls
    /// through to the default false, so a handler that asks on behalf of another user or about a
    /// different lyrics row is not silently handed this answer.
    /// </summary>
    public static Mock<ILyricsRepository> SetupHasLikedAsync(
        this Mock<ILyricsRepository> mock,
        Guid userId,
        Guid lyricsId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasLikedAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == lyricsId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Verifies the like-existence check was never reached, which is what the anonymous fast path
    /// means: no user id exists to ask about.
    /// </summary>
    public static void VerifyHasLikedNotCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(
            x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    public static void VerifyAddLikeCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.AddLikeAsync(It.IsAny<LyricsLikeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveLikeCalled(this Mock<ILyricsRepository> mock, Guid userId, Guid lyricsId)
    {
        mock.Verify(x => x.RemoveLikeAsync(userId, lyricsId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddShareCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.AddShareAsync(It.IsAny<LyricsShareEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static Mock<ILyricsRepository> SetupHasCountedViewSinceAsync(this Mock<ILyricsRepository> mock, bool result)
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

    public static Mock<ILyricsRepository> SetupCaptureViewEvent(
        this Mock<ILyricsRepository> mock,
        Action<LyricsViewEventEntity> capture
    )
    {
        mock.Setup(x => x.AddViewEventAsync(It.IsAny<LyricsViewEventEntity>(), It.IsAny<CancellationToken>()))
            .Callback<LyricsViewEventEntity, CancellationToken>((viewEvent, _) => capture(viewEvent))
            .Returns(Task.CompletedTask);
        return mock;
    }

    public static void VerifyAddViewEventCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(
            x => x.AddViewEventAsync(It.IsAny<LyricsViewEventEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyUpdateNotCalled(this Mock<ILyricsRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Never);
    }

    public static void VerifyHasCountedViewSinceNotCalled(this Mock<ILyricsRepository> mock)
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

    public static Mock<ILyricsRepository> SetupGetSimilarAsync(
        this Mock<ILyricsRepository> mock,
        Guid lyricsId,
        IReadOnlyList<LyricsEntity> similar
    )
    {
        mock.Setup(x => x.GetSimilarAsync(lyricsId, It.IsAny<CancellationToken>())).ReturnsAsync(similar);
        return mock;
    }

    public static Mock<ILyricsRepository> SetupGetLikedIdsAsync(
        this Mock<ILyricsRepository> mock,
        IReadOnlySet<Guid> likedIds
    )
    {
        mock.Setup(x =>
                x.GetLikedIdsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(likedIds);
        return mock;
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ILyricsRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<LyricsEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetPublishedByAlbumAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LyricsEntity>());
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<EnumContentStatus?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<LyricsEntity>(), 0));
        mock.Setup(x =>
                x.GetPublishedByArtistAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<LyricsEntity>(), 0));
        mock.Setup(x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.AddLikeAsync(It.IsAny<LyricsLikeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddShareAsync(It.IsAny<LyricsShareEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddViewEventAsync(It.IsAny<LyricsViewEventEntity>(), It.IsAny<CancellationToken>()))
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
        mock.Setup(x =>
                x.GetLikedIdsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((IReadOnlySet<Guid>)new HashSet<Guid>());
        mock.Setup(x => x.GetSimilarAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LyricsEntity>)new List<LyricsEntity>());
    }
}
