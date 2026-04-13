using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ContentOrderItemEntity"/>.
/// </summary>
public class ContentOrderItemEntityTests
{
    #region Update

    [Fact]
    public void Update_ShouldUpdateContentKind()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        EnumCoreContentType newKind = EnumCoreContentType.Video;

        // Act
        item.Update(
            contentKind: newKind,
            categoryId: null,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: null
        );

        // Assert
        item.ContentKind.Should().Be(newKind);
    }

    [Fact]
    public void Update_ShouldUpdateCategoryId()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        Guid newCategoryId = Guid.NewGuid();

        // Act
        item.Update(
            contentKind: null,
            categoryId: newCategoryId,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: null
        );

        // Assert
        item.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void Update_ShouldUpdatePromotionFields()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        Guid newPromoLevelId = Guid.NewGuid();
        const decimal newPromoPrice = 99.99m;

        // Act
        item.Update(
            contentKind: null,
            categoryId: null,
            promotionLevelId: newPromoLevelId,
            promoPriceSnapshotUsd: newPromoPrice,
            socialBoost: null,
            isBonus: null
        );

        // Assert
        item.PromotionLevelId.Should().Be(newPromoLevelId);
        item.PromoPriceSnapshotUsd.Should().Be(newPromoPrice);
    }

    [Fact]
    public void Update_ShouldUpdateSocialBoost()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        item.Update(
            contentKind: null,
            categoryId: null,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: true,
            isBonus: null
        );

        // Assert
        item.SocialBoost.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldUpdateIsBonus()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        item.Update(
            contentKind: null,
            categoryId: null,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: true
        );

        // Assert
        item.IsBonus.Should().BeTrue();
    }

    [Fact]
    public void Update_WithNullContentKind_ShouldKeepExisting()
    {
        // Arrange
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        EnumCoreContentType originalKind = item.ContentKind;

        // Act
        item.Update(
            contentKind: null,
            categoryId: null,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: null
        );

        // Assert
        item.ContentKind.Should().Be(originalKind);
    }

    [Fact]
    public void Update_WithNullCategoryId_ShouldKeepExisting()
    {
        // Arrange
        Guid originalCategoryId = Guid.NewGuid();
        ContentOrderItemEntity item = ContentOrderItemFactory.Create(Guid.NewGuid(), originalCategoryId);

        // Act
        item.Update(
            contentKind: null,
            categoryId: null,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: null,
            isBonus: null
        );

        // Assert
        item.CategoryId.Should().Be(originalCategoryId);
    }

    #endregion
}
