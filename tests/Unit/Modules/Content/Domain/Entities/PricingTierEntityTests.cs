using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="PricingTierEntity"/>.
/// </summary>
public class PricingTierEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidName_ShouldCreatePricingTier()
    {
        // Arrange
        var id = Guid.NewGuid();
        string name = TestConstants.Content.PricingTier.ValidName;

        // Act
        var entity = PricingTierEntity.Create(id, name, null);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Description.Should().BeNull();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Arrange
        string description = TestConstants.Content.PricingTier.ValidDescription;

        // Act
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, description);

        // Assert
        entity.Description.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () => PricingTierEntity.Create(Guid.NewGuid(), invalidName!, null);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidValues_ShouldUpdate()
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);

        // Act
        entity.Update(
            TestConstants.Content.PricingTier.AnotherValidName,
            TestConstants.Content.PricingTier.ValidDescription
        );

        // Assert
        entity.Name.Should().Be(TestConstants.Content.PricingTier.AnotherValidName);
        entity.Description.Should().Be(TestConstants.Content.PricingTier.ValidDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);

        // Act
        Action act = () => entity.Update(invalidName!, null);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Update_WithNullDescription_ShouldClearDescription()
    {
        // Arrange
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.PricingTier.ValidName,
            TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        entity.Update(TestConstants.Content.PricingTier.ValidName, null);

        // Assert
        entity.Description.Should().BeNull();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);
        entity.Deactivate();

        // Act
        bool result = entity.Activate();

        // Assert
        result.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);

        // Act
        bool result = entity.Activate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);

        // Act
        bool result = entity.Deactivate();

        // Assert
        result.Should().BeTrue();
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.Content.PricingTier.ValidName, null);
        entity.Deactivate();

        // Act
        bool result = entity.Deactivate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
