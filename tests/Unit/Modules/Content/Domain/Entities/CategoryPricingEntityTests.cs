using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="CategoryPricingEntity"/>.
/// </summary>
public class CategoryPricingEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreateCategoryPricing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        decimal price = TestConstants.CategoryPricing.ValidPriceUsd;

        // Act
        var entity = CategoryPricingEntity.Create(
            id,
            categoryId,
            tierId,
            price,
            TestErrorsFactory.CreateCategoryErrors()
        );

        // Assert
        entity.Id.Should().Be(id);
        entity.CategoryId.Should().Be(categoryId);
        entity.PricingTierId.Should().Be(tierId);
        entity.PriceUsd.Should().Be(price);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Act
        var entity = CategoryPricingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.CategoryPricing.ZeroPriceUsd,
            TestErrorsFactory.CreateCategoryErrors()
        );

        // Assert
        entity.PriceUsd.Should().Be(0m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNegativePrice_ShouldThrowBadRequestException(decimal invalidPrice)
    {
        // Act
        Action act = () =>
            CategoryPricingEntity.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                invalidPrice,
                TestErrorsFactory.CreateCategoryErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region UpdatePrice Tests

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var entity = CategoryPricingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.CategoryPricing.ValidPriceUsd,
            TestErrorsFactory.CreateCategoryErrors()
        );

        // Act
        entity.UpdatePrice(TestConstants.CategoryPricing.UpdatedPriceUsd, TestErrorsFactory.CreateCategoryErrors());

        // Assert
        entity.PriceUsd.Should().Be(TestConstants.CategoryPricing.UpdatedPriceUsd);
    }

    [Fact]
    public void UpdatePrice_WithZeroPrice_ShouldSucceed()
    {
        // Arrange
        var entity = CategoryPricingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.CategoryPricing.ValidPriceUsd,
            TestErrorsFactory.CreateCategoryErrors()
        );

        // Act
        entity.UpdatePrice(0m, TestErrorsFactory.CreateCategoryErrors());

        // Assert
        entity.PriceUsd.Should().Be(0m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-50)]
    public void UpdatePrice_WithNegativePrice_ShouldThrowBadRequestException(decimal invalidPrice)
    {
        // Arrange
        var entity = CategoryPricingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TestConstants.CategoryPricing.ValidPriceUsd,
            TestErrorsFactory.CreateCategoryErrors()
        );

        // Act
        Action act = () => entity.UpdatePrice(invalidPrice, TestErrorsFactory.CreateCategoryErrors());

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
