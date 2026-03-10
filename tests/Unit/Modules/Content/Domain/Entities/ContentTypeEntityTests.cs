using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ContentTypeEntity"/>.
/// </summary>
public class ContentTypeEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidName_ShouldCreateContentType()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.ContentType.ValidName;

        // Act
        ContentTypeEntity entity = ContentTypeEntity.Create(id, name);

        // Assert
        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () => ContentTypeEntity.Create(Guid.NewGuid(), invalidName!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidName_ShouldUpdateName()
    {
        // Arrange
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
        );
        string newName = TestConstants.Content.ContentType.AnotherValidName;

        // Act
        entity.Update(newName);

        // Assert
        entity.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
        );

        // Act
        Action act = () => entity.Update(invalidName!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrueAndSetIsActiveTrue()
    {
        // Arrange
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
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
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
        );

        // Act
        bool result = entity.Activate();

        // Assert
        result.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrueAndSetIsActiveFalse()
    {
        // Arrange
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
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
        ContentTypeEntity entity = ContentTypeEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.ContentType.ValidName
        );
        entity.Deactivate();

        // Act
        bool result = entity.Deactivate();

        // Assert
        result.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
    }

    #endregion
}
