using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ICategoryRepository"/>.
/// </summary>
public static class MockCategoryRepository
{
    /// <summary>
    /// Creates a new mock instance of ICategoryRepository with default setups.
    /// </summary>
    public static Mock<ICategoryRepository> Create()
    {
        Mock<ICategoryRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetByIdOrThrow(
        this Mock<ICategoryRepository> mock,
        CategoryEntity entity
    )
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetByIdOrThrowNotFound(this Mock<ICategoryRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetByIdAsync(
        this Mock<ICategoryRepository> mock,
        Guid id,
        CategoryEntity? entity
    )
    {
        mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetBySlug(
        this Mock<ICategoryRepository> mock,
        string slug,
        CategoryEntity? entity
    )
    {
        mock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetAllAsync(
        this Mock<ICategoryRepository> mock,
        List<CategoryEntity> list,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((list, totalCount));
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetActiveByContentType(
        this Mock<ICategoryRepository> mock,
        Guid? contentTypeId,
        IReadOnlyList<CategoryEntity> list
    )
    {
        mock.Setup(x => x.GetActiveByContentTypeAsync(contentTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetPricingByCategory(
        this Mock<ICategoryRepository> mock,
        Guid categoryId,
        IReadOnlyList<CategoryPricingEntity> list
    )
    {
        mock.Setup(x => x.GetPricingByCategoryAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetPricing(
        this Mock<ICategoryRepository> mock,
        Guid categoryId,
        Guid tierId,
        CategoryPricingEntity? pricing
    )
    {
        mock.Setup(x => x.GetPricingAsync(categoryId, tierId, It.IsAny<CancellationToken>())).ReturnsAsync(pricing);
        return mock;
    }

    public static void VerifyAddCalled(this Mock<ICategoryRepository> mock)
    {
        mock.Verify(x => x.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddPricingCalled(this Mock<ICategoryRepository> mock)
    {
        mock.Verify(
            x => x.AddPricingAsync(It.IsAny<CategoryPricingEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyRemovePricingCalled(this Mock<ICategoryRepository> mock, CategoryPricingEntity pricing)
    {
        mock.Verify(x => x.RemovePricing(pricing), Times.Once);
    }

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<ICategoryRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddPricingAsync(It.IsAny<CategoryPricingEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x =>
                x.GetAllAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<CategoryEntity>(), 0));
        mock.Setup(x => x.GetActiveByContentTypeAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CategoryEntity>());
        mock.Setup(x => x.GetPricingByCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CategoryPricingEntity>());
        mock.Setup(x => x.GetPinnedToFeedCategoriesAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CategoryEntity>());
    }

    /// <summary>
    /// Sets up GetDefaultLyricsCategoryAsync to return the specified category.
    /// </summary>
    public static Mock<ICategoryRepository> SetupGetDefaultLyricsCategory(
        this Mock<ICategoryRepository> mock,
        CategoryEntity? category
    )
    {
        mock.Setup(x => x.GetDefaultLyricsCategoryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(category);
        return mock;
    }

    public static Mock<ICategoryRepository> SetupGetGossipCategory(
        this Mock<ICategoryRepository> mock,
        CategoryEntity? category
    )
    {
        mock.Setup(x => x.GetGossipCategoryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(category);
        return mock;
    }

    /// <summary>
    /// Sets up GetExclusiveCategoryAsync to return the specified category.
    /// </summary>
    public static Mock<ICategoryRepository> SetupGetExclusiveCategory(
        this Mock<ICategoryRepository> mock,
        CategoryEntity? category
    )
    {
        mock.Setup(x => x.GetExclusiveCategoryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(category);
        return mock;
    }

    /// <summary>
    /// Sets up GetPinnedToFeedCategoriesAsync to return the given list for any content-type filter.
    /// </summary>
    public static Mock<ICategoryRepository> SetupGetPinnedToFeedCategories(
        this Mock<ICategoryRepository> mock,
        IReadOnlyList<CategoryEntity> categories
    )
    {
        mock.Setup(x => x.GetPinnedToFeedCategoriesAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        return mock;
    }
}
