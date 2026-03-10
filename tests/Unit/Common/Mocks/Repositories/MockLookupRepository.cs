using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ILookupRepository"/>.
/// </summary>
public static class MockLookupRepository
{
    /// <summary>
    /// Creates a new mock instance of ILookupRepository with default setups.
    /// </summary>
    public static Mock<ILookupRepository> Create()
    {
        Mock<ILookupRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    // ──── ContentType ────────────────────────────────────────────────────────

    public static Mock<ILookupRepository> SetupContentTypeExistsByName(
        this Mock<ILookupRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.ContentTypeExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetContentTypeByIdOrThrow(
        this Mock<ILookupRepository> mock,
        ContentTypeEntity entity
    )
    {
        mock.Setup(x => x.GetContentTypeByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetContentTypeByIdOrThrowNotFound(
        this Mock<ILookupRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetContentTypeByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"ContentType with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetAllContentTypes(
        this Mock<ILookupRepository> mock,
        IReadOnlyList<ContentTypeEntity> list
    )
    {
        mock.Setup(x => x.GetAllContentTypesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddContentTypeCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyAddContentTypeNotCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // ──── PricingTier ────────────────────────────────────────────────────────

    public static Mock<ILookupRepository> SetupPricingTierExistsByName(
        this Mock<ILookupRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.PricingTierExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetPricingTierByIdOrThrow(
        this Mock<ILookupRepository> mock,
        PricingTierEntity entity
    )
    {
        mock.Setup(x => x.GetPricingTierByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetPricingTierByIdOrThrowNotFound(
        this Mock<ILookupRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetPricingTierByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"PricingTier with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetAllPricingTiers(
        this Mock<ILookupRepository> mock,
        IReadOnlyList<PricingTierEntity> list
    )
    {
        mock.Setup(x => x.GetAllPricingTiersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddPricingTierCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddPricingTierAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyAddPricingTierNotCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddPricingTierAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // ──── PromotionLevel ─────────────────────────────────────────────────────

    public static Mock<ILookupRepository> SetupPromotionLevelExistsByName(
        this Mock<ILookupRepository> mock,
        string name,
        bool exists
    )
    {
        mock.Setup(x => x.PromotionLevelExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetPromotionLevelByIdOrThrow(
        this Mock<ILookupRepository> mock,
        PromotionLevelEntity entity
    )
    {
        mock.Setup(x => x.GetPromotionLevelByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetPromotionLevelByIdOrThrowNotFound(
        this Mock<ILookupRepository> mock,
        Guid id
    )
    {
        mock.Setup(x => x.GetPromotionLevelByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"PromotionLevel with id '{id}' was not found."));
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetAllPromotionLevels(
        this Mock<ILookupRepository> mock,
        IReadOnlyList<PromotionLevelEntity> list
    )
    {
        mock.Setup(x => x.GetAllPromotionLevelsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetActivePromotionLevels(
        this Mock<ILookupRepository> mock,
        IReadOnlyList<PromotionLevelEntity> list
    )
    {
        mock.Setup(x => x.GetActivePromotionLevelsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddPromotionLevelCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddPromotionLevelAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyAddPromotionLevelNotCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(
            x => x.AddPromotionLevelAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    // ──── Tag ────────────────────────────────────────────────────────────────

    public static Mock<ILookupRepository> SetupGetTagBySlug(
        this Mock<ILookupRepository> mock,
        string slug,
        TagEntity? tag
    )
    {
        mock.Setup(x => x.GetTagBySlugAsync(slug, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetAllTags(
        this Mock<ILookupRepository> mock,
        IReadOnlyList<TagEntity> list
    )
    {
        mock.Setup(x => x.GetAllTagsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(list);
        return mock;
    }

    public static void VerifyAddTagCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyAddTagNotCalled(this Mock<ILookupRepository> mock)
    {
        mock.Verify(x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ──── Defaults ───────────────────────────────────────────────────────────

    private static void SetupDefaults(Mock<ILookupRepository> mock)
    {
        // ContentType
        mock.Setup(x => x.ContentTypeExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllContentTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentTypeEntity>());

        // PricingTier
        mock.Setup(x => x.PricingTierExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.AddPricingTierAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllPricingTiersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PricingTierEntity>());

        // PromotionLevel
        mock.Setup(x => x.PromotionLevelExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mock.Setup(x => x.AddPromotionLevelAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllPromotionLevelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PromotionLevelEntity>());
        mock.Setup(x => x.GetActivePromotionLevelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PromotionLevelEntity>());

        // Tag
        mock.Setup(x => x.GetTagBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TagEntity?)null);
        mock.Setup(x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.GetAllTagsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagEntity>());
    }
}
