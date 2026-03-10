using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PackageSlotEntity"/>.
/// </summary>
public class PackageSlotEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreatePackageSlot()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid packageId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        int quantity = TestConstants.Content.PackageSlot.ValidQuantity;

        // Act
        PackageSlotEntity entity = PackageSlotEntity.Create(id, packageId, categoryId, isRequired: true, quantity);

        // Assert
        entity.Id.Should().Be(id);
        entity.PackageId.Should().Be(packageId);
        entity.CategoryId.Should().Be(categoryId);
        entity.IsRequired.Should().BeTrue();
        entity.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Create_WithNullCategoryId_ShouldCreateOpenSlot()
    {
        // Act
        PackageSlotEntity entity = PackageSlotEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            TestConstants.Content.PackageSlot.ValidQuantity
        );

        // Assert
        entity.CategoryId.Should().BeNull();
        entity.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Create_WithMultipleQuantity_ShouldSetQuantity()
    {
        // Act
        PackageSlotEntity entity = PackageSlotEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            TestConstants.Content.PackageSlot.AnotherValidQuantity
        );

        // Assert
        entity.Quantity.Should().Be(TestConstants.Content.PackageSlot.AnotherValidQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithQuantityLessThanOrEqualToZero_ShouldThrowBadRequestException(int invalidQuantity)
    {
        // Act
        Action act = () => PackageSlotEntity.Create(Guid.NewGuid(), Guid.NewGuid(), null, false, invalidQuantity);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
