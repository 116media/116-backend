using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
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
        var id = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        int quantity = TestConstants.PackageSlot.ValidQuantity;

        // Act
        var entity = PackageSlotEntity.Create(
            id,
            packageId,
            categoryId,
            isRequired: true,
            quantity,
            TestErrorsFactory.CreatePackageErrors()
        );

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
        var entity = PackageSlotEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            TestConstants.PackageSlot.ValidQuantity,
            TestErrorsFactory.CreatePackageErrors()
        );

        // Assert
        entity.CategoryId.Should().BeNull();
        entity.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Create_WithMultipleQuantity_ShouldSetQuantity()
    {
        // Act
        var entity = PackageSlotEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            false,
            TestConstants.PackageSlot.AnotherValidQuantity,
            TestErrorsFactory.CreatePackageErrors()
        );

        // Assert
        entity.Quantity.Should().Be(TestConstants.PackageSlot.AnotherValidQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithQuantityLessThanOrEqualToZero_ShouldThrowBadRequestException(int invalidQuantity)
    {
        // Act
        Action act = () =>
            PackageSlotEntity.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                false,
                invalidQuantity,
                TestErrorsFactory.CreatePackageErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
