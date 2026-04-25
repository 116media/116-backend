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
        var id = Guid.NewGuid();
        string name = TestConstants.Content.Package.ValidName;
        string description = TestConstants.Content.Package.ValidDescription;

        // Act
        var entity = PackageEntity.Create(id, name, description);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.Description.Should().Be(description);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithDescription_ShouldSetDescription()
    {
        // Act
        var entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription
        );

        // Assert
        entity.Description.Should().Be(TestConstants.Content.Package.ValidDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () =>
            PackageEntity.Create(Guid.NewGuid(), invalidName!, TestConstants.Content.Package.ValidDescription);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate / Deactivate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        var entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription
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
        var entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription
        );

        // Act & Assert
        entity.Activate().Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        var entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription
        );

        // Act & Assert
        entity.Deactivate().Should().BeTrue();
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        var entity = PackageEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Package.ValidName,
            TestConstants.Content.Package.ValidDescription
        );
        entity.Deactivate();

        // Act & Assert
        entity.Deactivate().Should().BeFalse();
    }

    #endregion
}
