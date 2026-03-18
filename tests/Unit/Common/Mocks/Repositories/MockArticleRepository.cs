using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IArticleRepository"/>.
/// </summary>
public static class MockArticleRepository
{
    /// <summary>
    /// Creates a new mock instance of IArticleRepository with safe default setups.
    /// </summary>
    public static Mock<IArticleRepository> Create()
    {
        Mock<IArticleRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetByIdOrThrow(this Mock<IArticleRepository> mock, ArticleEntity entity)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetByIdOrThrowNotFound(this Mock<IArticleRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Article with id '{id}' was not found."));
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetByIdAsync(
        this Mock<IArticleRepository> mock,
        Guid id,
        ArticleEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetByOrderItemIdAsync(
        this Mock<IArticleRepository> mock,
        Guid orderItemId,
        ArticleEntity? entity
    )
    {
        mock.Setup(x => x.GetByOrderItemIdAsync(orderItemId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetBySlug(
        this Mock<IArticleRepository> mock,
        string slug,
        ArticleEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetAllAsync(
        this Mock<IArticleRepository> mock,
        List<ArticleEntity> articles,
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
            .ReturnsAsync((articles, totalCount));
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetFeaturedAsync(
        this Mock<IArticleRepository> mock,
        IReadOnlyList<ArticleEntity> articles
    )
    {
        mock.Setup(x => x.GetFeaturedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(articles);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetAbandonedDraftsAsync(
        this Mock<IArticleRepository> mock,
        IReadOnlyList<ArticleEntity> articles
    )
    {
        mock.Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(articles);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetImagesByArticleId(
        this Mock<IArticleRepository> mock,
        Guid articleId,
        IReadOnlyList<ArticleImageEntity> images
    )
    {
        mock.Setup(x => x.GetImagesByArticleIdAsync(articleId, It.IsAny<CancellationToken>())).ReturnsAsync(images);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetTagsByArticleId(
        this Mock<IArticleRepository> mock,
        Guid articleId,
        IReadOnlyList<ArticleTagEntity> tags
    )
    {
        mock.Setup(x => x.GetTagsByArticleIdAsync(articleId, It.IsAny<CancellationToken>())).ReturnsAsync(tags);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyUpdateCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.Update(It.IsAny<ArticleEntity>()), Times.Once);
    }

    public static void VerifyRemoveCalled(this Mock<IArticleRepository> mock, ArticleEntity article)
    {
        mock.Verify(x => x.Remove(article), Times.Once);
    }

    public static void VerifyAddImageCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveImagesCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.RemoveImages(It.IsAny<IEnumerable<ArticleImageEntity>>()), Times.Once);
    }

    public static void VerifyAddTagCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    public static void VerifyRemoveTagCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.RemoveTag(It.IsAny<ArticleTagEntity>()), Times.Once);
    }

    public static Mock<IArticleRepository> SetupHasLikedAsync(this Mock<IArticleRepository> mock, bool result)
    {
        mock.Setup(x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IArticleRepository> SetupHasBookmarkedAsync(this Mock<IArticleRepository> mock, bool result)
    {
        mock.Setup(x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetCommentByIdAsync(
        this Mock<IArticleRepository> mock,
        ArticleCommentEntity? comment
    )
    {
        mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(comment);
        return mock;
    }

    public static Mock<IArticleRepository> SetupGetCommentsAsync(
        this Mock<IArticleRepository> mock,
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

    public static Mock<IArticleRepository> SetupGetBookmarkedArticlesAsync(
        this Mock<IArticleRepository> mock,
        List<ArticleEntity> articles,
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
            .ReturnsAsync((articles, totalCount));
        return mock;
    }

    public static void VerifyAddLikeCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.AddLikeAsync(It.IsAny<ArticleLikeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyRemoveLikeCalled(this Mock<IArticleRepository> mock, Guid userId, Guid articleId)
    {
        mock.Verify(x => x.RemoveLikeAsync(userId, articleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddBookmarkCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(
            x => x.AddBookmarkAsync(It.IsAny<ArticleBookmarkEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemoveBookmarkCalled(this Mock<IArticleRepository> mock, Guid userId, Guid articleId)
    {
        mock.Verify(x => x.RemoveBookmarkAsync(userId, articleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddShareCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.AddShareAsync(It.IsAny<ArticleShareEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddCommentCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(
            x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyUpdateCommentCalled(this Mock<IArticleRepository> mock)
    {
        mock.Verify(x => x.UpdateComment(It.IsAny<ArticleCommentEntity>()), Times.Once);
    }

    private static void SetupDefaults(Mock<IArticleRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetByOrderItemIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleEntity?)null);
        mock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleEntity?)null);
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleEntity?)null);
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
            .ReturnsAsync((new List<ArticleEntity>(), 0));
        mock.Setup(x => x.GetFeaturedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleEntity>());
        mock.Setup(x => x.GetAbandonedDraftsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleEntity>());
        mock.Setup(x => x.GetImagesByArticleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleImageEntity>());
        mock.Setup(x => x.GetTagsByArticleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArticleTagEntity>());
        mock.Setup(x => x.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.GetCommentByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleCommentEntity?)null);
        mock.Setup(x =>
                x.GetCommentsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((new List<ArticleCommentEntity>(), 0));
        mock.Setup(x =>
                x.GetBookmarkedArticlesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<ArticleEntity>(), 0));
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
        mock.Setup(x => x.AddCommentAsync(It.IsAny<ArticleCommentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
