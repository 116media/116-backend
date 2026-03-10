using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PackageEntity"/>.
/// </summary>
public class PackageEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreatePackage()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.Package.ValidName;
        decimal price = TestConstants.Content.Package.ValidFlatPriceUsd;

        // Act
        PackageEntity entity = PackageEntity.Create(id, name, null, price);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Description.Should().BeNull();
        entity.FlatPriceUsd.Should().Be(price);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Act
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription,
            TestConstants.Content.Package.ValidFlatPriceUsd
        );

        // Assert
        entity.Description.Should().Be(TestConstants.Content.Package.ValidDescription);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Act
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            null,
            TestConstants.Content.Package.ZeroFlatPriceUsd
        );

        // Assert
        entity.FlatPriceUsd.Should().Be(0m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () => PackageEntity.Create(Guid.NewGuid(), invalidName!, null, 100m);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNegativePrice_ShouldThrowBadRequestException(decimal invalidPrice)
    {
        // Act
        Action act = () =>
            PackageEntity.Create(Guid.NewGuid(), TestConstants.Content.Package.ValidName, null, invalidPrice);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate / Deactivate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            null,
            100m
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
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            null,
            100m
        );

        // Act & Assert
        entity.Activate().Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            null,
            100m
        );

        // Act & Assert
        entity.Deactivate().Should().BeTrue();
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        PackageEntity entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            null,
            100m
        );
        entity.Deactivate();

        // Act & Assert
        entity.Deactivate().Should().BeFalse();
    }

    #endregion
}
