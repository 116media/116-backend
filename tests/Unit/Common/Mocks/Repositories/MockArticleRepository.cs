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

    private static void SetupDefaults(Mock<IArticleRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<ArticleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddImageAsync(It.IsAny<ArticleImageEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddTagAsync(It.IsAny<ArticleTagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
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
    }
}
