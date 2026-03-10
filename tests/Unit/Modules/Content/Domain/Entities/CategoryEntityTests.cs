using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="CategoryEntity"/>.
/// </summary>
public class CategoryEntityTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidValues_ShouldCreateCategory()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.Category.ValidName;
        string slug = TestConstants.Content.Category.ValidSlug;
        string description = TestConstants.Content.Category.ValidDescription;

        // Act
        CategoryEntity entity = CategoryEntity.Create(id, ContentTypeId, name, slug, description, isFree: false);

        // Assert
        entity.Id.Should().Be(id);
        entity.ContentTypeId.Should().Be(ContentTypeId);
        entity.Name.Should().Be(name);
        entity.Slug.Should().Be(slug);
        entity.Description.Should().Be(description);
        entity.IsFree.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Act
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Assert
        entity.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithIsFreeTrue_ShouldMarkAsFree()
    {
        // Act
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            true
        );

        // Assert
        entity.IsFree.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () =>
            CategoryEntity.Create(
                Guid.NewGuid(),
                ContentTypeId,
                invalidName!,
                TestConstants.Content.Category.ValidSlug,
                null,
                false
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidSlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            CategoryEntity.Create(
                Guid.NewGuid(),
                ContentTypeId,
                TestConstants.Content.Category.ValidName,
                invalidSlug!,
                null,
                false
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
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Act
        entity.Update(
            TestConstants.Content.Category.AnotherValidName,
            TestConstants.Content.Category.AnotherValidSlug,
            TestConstants.Content.Category.ValidDescription
        );

        // Assert
        entity.Name.Should().Be(TestConstants.Content.Category.AnotherValidName);
        entity.Slug.Should().Be(TestConstants.Content.Category.AnotherValidSlug);
        entity.Description.Should().Be(TestConstants.Content.Category.ValidDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Act
        Action act = () => entity.Update(invalidName!, TestConstants.Content.Category.ValidSlug, null);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidSlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Arrange
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Act
        Action act = () => entity.Update(TestConstants.Content.Category.ValidName, invalidSlug!, null);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate / Deactivate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
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
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Act & Assert
        entity.Activate().Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );

        // Act & Assert
        entity.Deactivate().Should().BeTrue();
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity entity = CategoryEntity.Create(
            Guid.NewGuid(),
            ContentTypeId,
            TestConstants.Content.Category.ValidName,
            TestConstants.Content.Category.ValidSlug,
            null,
            false
        );
        entity.Deactivate();

        // Act & Assert
        entity.Deactivate().Should().BeFalse();
    }

    #endregion
}
