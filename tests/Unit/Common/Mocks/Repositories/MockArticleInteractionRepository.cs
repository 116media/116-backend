using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IArticleInteractionRepository"/>.
/// </summary>
public static class MockArticleInteractionRepository
{
    /// <summary>
    /// Creates a new mock instance of IArticleInteractionRepository with default setups.
    /// </summary>
    public static Mock<IArticleInteractionRepository> Create()
    {
        Mock<IArticleInteractionRepository> mock = new();
        mock.Setup(x => x.ExistsOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x =>
                x.GetLikedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ArticleActivity>(), 0));
        mock.Setup(x =>
                x.GetSharedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ArticleActivity>(), 0));
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
            .ReturnsAsync((new HashSet<Guid>(), new HashSet<Guid>()));
        mock.Setup(x =>
                x.GetBookmarkedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<BookmarkedArticleActivity>(), 0));
        mock.Setup(x => x.AddLikeAsync(It.IsAny<ArticleLikeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddBookmarkAsync(It.IsAny<ArticleBookmarkEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveBookmarkAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddShareAsync(It.IsAny<ArticleShareEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Answers the like-existence check for one user and article pair only. Any other pair falls
    /// through to the default false, so a handler that asks on behalf of another user or about a
    /// different article is not silently handed this answer.
    /// </summary>
    public static Mock<IArticleInteractionRepository> SetupHasLikedAsync(
        this Mock<IArticleInteractionRepository> mock,
        Guid userId,
        Guid articleId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasLikedAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == articleId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Answers the bookmark-existence check for one user and article pair only. Any other pair
    /// falls through to the default false, so a handler that asks on behalf of another user or
    /// about a different article is not silently handed this answer.
    /// </summary>
    public static Mock<IArticleInteractionRepository> SetupHasBookmarkedAsync(
        this Mock<IArticleInteractionRepository> mock,
        Guid userId,
        Guid articleId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasBookmarkedAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == articleId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Configures GetLikedAndBookmarkedIdsAsync to return the given liked and bookmarked id sets
    /// for any user/id-list input.
    /// </summary>
    public static Mock<IArticleInteractionRepository> SetupGetLikedAndBookmarkedIds(
        this Mock<IArticleInteractionRepository> mock,
        HashSet<Guid> likedIds,
        HashSet<Guid> bookmarkedIds
    )
    {
        mock.Setup(x =>
                x.GetLikedAndBookmarkedIdsAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((likedIds, bookmarkedIds));
        return mock;
    }

    /// <summary>
    /// Verifies GetLikedAndBookmarkedIdsAsync was invoked exactly the given number of times
    /// with a non-null user id.
    /// </summary>
    public static void VerifyGetLikedAndBookmarkedIdsCalledWithUser(
        this Mock<IArticleInteractionRepository> mock,
        Times times
    )
    {
        mock.Verify(
            x =>
                x.GetLikedAndBookmarkedIdsAsync(
                    It.Is<Guid?>(userId => userId != null),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                ),
            times
        );
    }

    /// <summary>
    /// Verifies neither HasLikedAsync nor HasBookmarkedAsync was invoked (anonymous fast path).
    /// </summary>
    public static void VerifyExistenceChecksNotCalled(this Mock<IArticleInteractionRepository> mock)
    {
        mock.Verify(
            x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        mock.Verify(
            x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    public static Mock<IArticleInteractionRepository> SetupGetBookmarkedArticlesAsync(
        this Mock<IArticleInteractionRepository> mock,
        List<BookmarkedArticleActivity> activities,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetBookmarkedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((activities, totalCount));
        return mock;
    }

    public static Mock<IArticleInteractionRepository> SetupGetLikedArticlesAsync(
        this Mock<IArticleInteractionRepository> mock,
        List<ArticleActivity> activities,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetLikedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((activities, totalCount));
        return mock;
    }

    public static Mock<IArticleInteractionRepository> SetupGetSharedArticlesAsync(
        this Mock<IArticleInteractionRepository> mock,
        List<ArticleActivity> activities,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetSharedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((activities, totalCount));
        return mock;
    }

    public static void VerifyAddLikeCalled(this Mock<IArticleInteractionRepository> mock)
    {
        mock.Verify(x => x.AddLikeAsync(It.IsAny<ArticleLikeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveLikeCalled(
        this Mock<IArticleInteractionRepository> mock,
        Guid userId,
        Guid articleId
    )
    {
        mock.Verify(x => x.RemoveLikeAsync(userId, articleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddBookmarkCalled(this Mock<IArticleInteractionRepository> mock)
    {
        mock.Verify(
            x => x.AddBookmarkAsync(It.IsAny<ArticleBookmarkEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemoveBookmarkCalled(
        this Mock<IArticleInteractionRepository> mock,
        Guid userId,
        Guid articleId
    )
    {
        mock.Verify(x => x.RemoveBookmarkAsync(userId, articleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddShareCalled(this Mock<IArticleInteractionRepository> mock)
    {
        mock.Verify(x => x.AddShareAsync(It.IsAny<ArticleShareEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static Mock<IArticleInteractionRepository> SetupExistsOrThrow(
        this Mock<IArticleInteractionRepository> mock,
        Guid articleId
    )
    {
        mock.Setup(x => x.ExistsOrThrowAsync(articleId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return mock;
    }

    public static Mock<IArticleInteractionRepository> SetupExistsOrThrowNotFound(
        this Mock<IArticleInteractionRepository> mock,
        Guid articleId
    )
    {
        mock.Setup(x => x.ExistsOrThrowAsync(articleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(ArticleEntity), articleId));
        return mock;
    }
}
