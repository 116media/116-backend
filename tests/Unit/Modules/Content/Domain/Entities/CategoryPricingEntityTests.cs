using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="CategoryPricingEntity" />, which is a member of the category
/// aggregate: every arrangement and assertion goes through <see cref="CategoryEntity" />,
/// the only writer of its rows.
/// </summary>
public class CategoryPricingEntityTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region SetPricing Tests

    [Fact]
    public void SetPricing_OnAnUnpricedTier_ShouldAddTheRowCarryingTheCategoryAndTier()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();

        // Act
        bool changed = category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);

        // Assert
        changed.Should().BeTrue();
        CategoryPricingEntity pricing = category.Pricing.Should().ContainSingle().Subject;
        pricing.CategoryId.Should().Be(category.Id);
        pricing.PricingTierId.Should().Be(pricingTierId);
        pricing.PriceUsd.Amount.Should().Be(25m);
        pricing.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void SetPricing_OnAPricedTier_ShouldRepriceTheSameRow()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);
        Guid originalRowId = category.Pricing.Single().Id;

        // Act
        bool changed = category.SetPricing(pricingTierId: pricingTierId, priceUsd: 30m);

        // Assert
        changed.Should().BeTrue();
        category.Pricing.Should().ContainSingle();
        category.Pricing.Single().Id.Should().Be(originalRowId);
        category.Pricing.Single().PriceUsd.Amount.Should().Be(30m);
    }

    [Fact]
    public void SetPricing_WithTheSamePrice_ShouldReportNoChange()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);

        // Act
        bool changed = category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);

        // Assert
        changed.Should().BeFalse();
    }

    [Fact]
    public void SetPricing_ForTwoTiers_ShouldKeepBothRows()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);

        // Act
        category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: 25m);
        category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: 40m);

        // Assert
        category.Pricing.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void SetPricing_WithANegativePrice_ShouldThrow(decimal invalidPrice)
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);

        // Act
        Action act = () => category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: invalidPrice);

        // Assert
        act.Should()
            .Throw<ContentRuleException>()
            .Where(exception => exception.Code == ContentRuleCodes.CategoryPriceMustBeNonNegative);
        category.Pricing.Should().BeEmpty();
    }

    [Fact]
    public void SetPricing_WithZero_ShouldBeAllowed()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);

        // Act
        category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: 0m);

        // Assert
        category.Pricing.Single().PriceUsd.Amount.Should().Be(0m);
    }

    #endregion

    #region RemovePricing Tests

    [Fact]
    public void RemovePricing_OnAPricedTier_ShouldDropTheRow()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);

        // Act
        bool removed = category.RemovePricing(pricingTierId: pricingTierId);

        // Assert
        removed.Should().BeTrue();
        category.Pricing.Should().BeEmpty();
    }

    [Fact]
    public void RemovePricing_OnAnUnpricedTier_ShouldReportNoChange()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);

        // Act
        bool removed = category.RemovePricing(pricingTierId: Guid.NewGuid());

        // Assert
        removed.Should().BeFalse();
    }

    #endregion

    #region FindPricing Tests

    [Fact]
    public void FindPricing_ShouldReturnOnlyTheRowForThatTier()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);
        category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: 40m);

        // Act
        CategoryPricingEntity? found = category.FindPricing(pricingTierId: pricingTierId);

        // Assert
        found.Should().NotBeNull();
        found!.PriceUsd.Amount.Should().Be(25m);
    }

    [Fact]
    public void FindPricing_ForAnUnpricedTier_ShouldReturnNull()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);

        // Act & Assert
        category.FindPricing(pricingTierId: Guid.NewGuid()).Should().BeNull();
    }

    #endregion

    #region Domain Events

    [Fact]
    public void SetPricing_OnAnUnpricedTier_ShouldRaiseCategoryChangedOnTheRoot()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        category.ClearDomainEvents();

        // Act
        category.SetPricing(pricingTierId: Guid.NewGuid(), priceUsd: 25m);

        // Assert
        category
            .DomainEvents.OfType<CategoryChangedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new CategoryChangedEvent(category.Id));
    }

    [Fact]
    public void SetPricing_WhenRepricing_ShouldRaiseCategoryChangedOnTheRoot()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);
        category.ClearDomainEvents();

        // Act
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 30m);

        // Assert
        category.DomainEvents.OfType<CategoryChangedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void SetPricing_WithAnUnchangedPrice_ShouldRaiseNothing()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);
        category.ClearDomainEvents();

        // Act
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);

        // Assert
        category.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemovePricing_ShouldRaiseCategoryChangedOnTheRoot()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var pricingTierId = Guid.NewGuid();
        category.SetPricing(pricingTierId: pricingTierId, priceUsd: 25m);
        category.ClearDomainEvents();

        // Act
        category.RemovePricing(pricingTierId: pricingTierId);

        // Assert
        category.DomainEvents.OfType<CategoryChangedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void RemovePricing_OnAnUnpricedTier_ShouldRaiseNothing()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        category.ClearDomainEvents();

        // Act
        category.RemovePricing(pricingTierId: Guid.NewGuid());

        // Assert
        category.DomainEvents.Should().BeEmpty();
    }

    #endregion
}
