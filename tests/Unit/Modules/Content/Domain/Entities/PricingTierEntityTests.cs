using _116.Content.Domain.Entities;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
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
        string name = TestConstants.PricingTier.ValidName;
        string description = TestConstants.PricingTier.ValidDescription;

        // Act
        var entity = PricingTierEntity.Create(id, name, description);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Description.Should().Be(description);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Arrange
        string description = TestConstants.PricingTier.ValidDescription;

        // Act
        var entity = PricingTierEntity.Create(Guid.NewGuid(), TestConstants.PricingTier.ValidName, description);

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
        Action act = () =>
            PricingTierEntity.Create(Guid.NewGuid(), invalidName!, TestConstants.PricingTier.ValidDescription);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PricingTierNameRequired);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidValues_ShouldUpdate()
    {
        // Arrange
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );

        // Act
        entity.Update(TestConstants.PricingTier.AnotherValidName, TestConstants.PricingTier.ValidDescription);

        // Assert
        entity.Name.Should().Be(TestConstants.PricingTier.AnotherValidName);
        entity.Description.Should().Be(TestConstants.PricingTier.ValidDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );

        // Act
        Action act = () => entity.Update(invalidName!, TestConstants.PricingTier.ValidDescription);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.PricingTierNameRequired);
    }

    [Fact]
    public void Update_WithNewDescription_ShouldUpdateDescription()
    {
        // Arrange
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );
        const string newDescription = "Updated pricing tier description.";

        // Act
        entity.Update(TestConstants.PricingTier.ValidName, newDescription);

        // Assert
        entity.Description.Should().Be(newDescription);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );
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
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );

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
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );

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
        var entity = PricingTierEntity.Create(
            Guid.NewGuid(),
            TestConstants.PricingTier.ValidName,
            TestConstants.PricingTier.ValidDescription
        );
        entity.Deactivate();

        // Act
        bool result = entity.Deactivate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
