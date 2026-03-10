using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PromotionLevelEntity"/>.
/// </summary>
public class PromotionLevelEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreatePromotionLevel()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.PromotionLevel.ValidName;
        int durationDays = TestConstants.Content.PromotionLevel.ValidDurationDays;
        decimal priceUsd = TestConstants.Content.PromotionLevel.ValidPriceUsd;

        // Act
        PromotionLevelEntity entity = PromotionLevelEntity.Create(id, name, durationDays, priceUsd);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.DurationDays.Should().Be(durationDays);
        entity.PriceUsd.Should().Be(priceUsd);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithZeroPriceUsd_ShouldSucceed()
    {
        // Act
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ZeroPriceUsd
        );

        // Assert
        entity.PriceUsd.Should().Be(0m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () =>
            PromotionLevelEntity.Create(
                Guid.NewGuid(),
                invalidName!,
                TestConstants.Content.PromotionLevel.ValidDurationDays,
                TestConstants.Content.PromotionLevel.ValidPriceUsd
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithDurationDaysLessThanOrEqualToZero_ShouldThrowBadRequestException(int invalidDuration)
    {
        // Act
        Action act = () =>
            PromotionLevelEntity.Create(
                Guid.NewGuid(),
                TestConstants.Content.PromotionLevel.ValidName,
                invalidDuration,
                TestConstants.Content.PromotionLevel.ValidPriceUsd
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNegativePriceUsd_ShouldThrowBadRequestException(decimal invalidPrice)
    {
        // Act
        Action act = () =>
            PromotionLevelEntity.Create(
                Guid.NewGuid(),
                TestConstants.Content.PromotionLevel.ValidName,
                TestConstants.Content.PromotionLevel.ValidDurationDays,
                invalidPrice
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidValues_ShouldUpdate()
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        entity.Update(TestConstants.Content.PromotionLevel.AnotherValidName, 14, 100m);

        // Assert
        entity.Name.Should().Be(TestConstants.Content.PromotionLevel.AnotherValidName);
        entity.DurationDays.Should().Be(14);
        entity.PriceUsd.Should().Be(100m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        Action act = () => entity.Update(invalidName!, TestConstants.Content.PromotionLevel.ValidDurationDays, 0m);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithInvalidDuration_ShouldThrowBadRequestException(int invalidDuration)
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        Action act = () => entity.Update(TestConstants.Content.PromotionLevel.ValidName, invalidDuration, 0m);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Update_WithNegativePriceUsd_ShouldThrowBadRequestException(decimal invalidPrice)
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        Action act = () =>
            entity.Update(
                TestConstants.Content.PromotionLevel.ValidName,
                TestConstants.Content.PromotionLevel.ValidDurationDays,
                invalidPrice
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate / Deactivate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );
        entity.Deactivate();

        // Act & Assert
        entity.Activate().Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act & Assert
        entity.Activate().Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act & Assert
        entity.Deactivate().Should().BeTrue();
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        PromotionLevelEntity entity = PromotionLevelEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PromotionLevel.ValidName,
            TestConstants.Content.PromotionLevel.ValidDurationDays,
            TestConstants.Content.PromotionLevel.ValidPriceUsd
        );
        entity.Deactivate();

        // Act & Assert
        entity.Deactivate().Should().BeFalse();
    }

    #endregion
}
