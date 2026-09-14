using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IArticleCommentRepository"/>.
/// </summary>
public static class MockArticleCommentRepository
{
    /// <summary>
    /// Creates a new mock instance of IArticleCommentRepository with default setups.
    /// </summary>
    public static Mock<IArticleCommentRepository> Create()
    {
        Mock<IArticleCommentRepository> mock = new();
        mock.Setup(x => x.ExistsOrThrowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x =>
                x.GetCommentedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<CommentedArticleActivity>(), 0));
        mock.Setup(x =>
                x.GetOwnCommentsForArticleAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ArticleCommentEntity>(), 0));
        mock.Setup(x =>
                x.GetCommentsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<ArticleCommentEntity>(), 0));
        mock.Setup(x =>
                x.GetRepliesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<ArticleCommentEntity>(), 0));
        mock.Setup(x => x.GetReplyCountsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());
        mock.Setup(x => x.HasLikedCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x =>
                x.GetLikedCommentIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new HashSet<Guid>());
        mock.Setup(x => x.AddCommentLikeAsync(It.IsAny<ArticleCommentLikeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.RemoveCommentLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    /// <summary>
    /// Sets up the comment lookup to return the comment only for its own id. Any other id falls
    /// through to the loose default, so a handler that looks up a different comment is not silently
    /// satisfied.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="comment">The comment returned for its own identifier.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IArticleCommentRepository> SetupGetCommentByIdAsync(
        this Mock<IArticleCommentRepository> mock,
        ArticleCommentEntity comment
    )
    {
        Guid commentId = comment.Id;
        mock.Setup(x => x.GetCommentByIdAsync(It.Is<Guid>(id => id == commentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for <paramref name="commentId" />. Naming the identifier is what separates
    /// "this comment does not exist" from "no comment lookup this handler makes can succeed".
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="commentId">The identifier that must resolve to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IArticleCommentRepository> SetupGetCommentByIdNotFound(
        this Mock<IArticleCommentRepository> mock,
        Guid commentId
    )
    {
        mock.Setup(x => x.GetCommentByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleCommentEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up the article-scoped comment lookup to return the comment only for its own id within
    /// the given article, so both a cross-article lookup and a wrong-comment lookup stay unanswered.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="comment">The comment returned for its own identifier.</param>
    /// <param name="articleId">The article the comment must be looked up under.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IArticleCommentRepository> SetupGetCommentByIdInArticleAsync(
        this Mock<IArticleCommentRepository> mock,
        ArticleCommentEntity comment,
        Guid articleId
    )
    {
        Guid commentId = comment.Id;
        mock.Setup(x =>
                x.GetCommentByIdAsync(
                    It.Is<Guid>(id => id == commentId),
                    It.Is<Guid>(id => id == articleId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(comment);
        return mock;
    }

    /// <summary>
    /// Arranges a miss for the given comment within the given article, naming both identifiers so
    /// the not-found branch is reached only for the pair the test declares.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    /// <param name="commentId">The comment identifier that must resolve to nothing.</param>
    /// <param name="articleId">The article the lookup is scoped to.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IArticleCommentRepository> SetupGetCommentByIdInArticleNotFound(
        this Mock<IArticleCommentRepository> mock,
        Guid commentId,
        Guid articleId
    )
    {
        mock.Setup(x => x.GetCommentByIdAsync(commentId, articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleCommentEntity?)null);
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupGetCommentsAsync(
        this Mock<IArticleCommentRepository> mock,
        List<ArticleCommentEntity> comments,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetCommentsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((comments, totalCount));
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupGetRepliesAsync(
        this Mock<IArticleCommentRepository> mock,
        List<ArticleCommentEntity> replies,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetRepliesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((replies, totalCount));
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupGetReplyCounts(
        this Mock<IArticleCommentRepository> mock,
        IReadOnlyDictionary<Guid, int> counts
    )
    {
        mock.Setup(x => x.GetReplyCountsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(counts);
        return mock;
    }

    /// <summary>
    /// Answers the comment-like existence check for one user and comment pair only. Any other pair
    /// falls through to the default false, so a handler that asks on behalf of another user or
    /// about a different comment is not silently handed this answer.
    /// </summary>
    public static Mock<IArticleCommentRepository> SetupHasLikedCommentAsync(
        this Mock<IArticleCommentRepository> mock,
        Guid userId,
        Guid commentId,
        bool result
    )
    {
        mock.Setup(x =>
                x.HasLikedCommentAsync(
                    It.Is<Guid>(id => id == userId),
                    It.Is<Guid>(id => id == commentId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupGetLikedCommentIds(
        this Mock<IArticleCommentRepository> mock,
        HashSet<Guid> commentIds
    )
    {
        mock.Setup(x =>
                x.GetLikedCommentIdsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(commentIds);
        return mock;
    }

    public static void VerifyAddCommentLikeCalled(this Mock<IArticleCommentRepository> mock, Times times)
    {
        mock.Verify(
            x => x.AddCommentLikeAsync(It.IsAny<ArticleCommentLikeEntity>(), It.IsAny<CancellationToken>()),
            times
        );
    }

    public static void VerifyRemoveCommentLikeCalled(this Mock<IArticleCommentRepository> mock, Times times)
    {
        mock.Verify(
            x => x.RemoveCommentLikeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            times
        );
    }

    public static Mock<IArticleCommentRepository> SetupGetCommentedArticlesAsync(
        this Mock<IArticleCommentRepository> mock,
        List<CommentedArticleActivity> activities,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetCommentedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((activities, totalCount));
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupGetOwnCommentsForArticleAsync(
        this Mock<IArticleCommentRepository> mock,
        List<ArticleCommentEntity> comments,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetOwnCommentsForArticleAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((comments, totalCount));
        return mock;
    }

    public static void VerifyAddCommentCalled(this Mock<IArticleCommentRepository> mock)
    {
        mock.Verify(
            x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static Mock<IArticleCommentRepository> SetupExistsOrThrow(
        this Mock<IArticleCommentRepository> mock,
        Guid articleId
    )
    {
        mock.Setup(x => x.ExistsOrThrowAsync(articleId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return mock;
    }

    public static Mock<IArticleCommentRepository> SetupExistsOrThrowNotFound(
        this Mock<IArticleCommentRepository> mock,
        Guid articleId
    )
    {
        mock.Setup(x => x.ExistsOrThrowAsync(articleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(ArticleEntity), articleId));
        return mock;
    }
}
