using _116.Content.Application.Catalog.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.Specifications;

/// <summary>
/// Unit tests for the category specification classes not already covered by
/// <see cref="CatalogSpecificationsTests"/> and <see cref="CategorySpecificationTests"/>.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
/// </summary>
public class CategorySpecificationsTests
{
    private static readonly Guid ContentTypeId = Guid.NewGuid();

    #region CategoryBySlugSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void CategoryBySlugSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new CategoryBySlugSpecification("music-videos");

        // Act
        Func<CategoryEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region DefaultLyricsCategorySpecification

    [Fact]
    public void DefaultLyricsCategorySpecification_WithActiveDefaultLyricsCategory_ShouldReturnTrue()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(ContentTypeId);
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DefaultLyricsCategorySpecification_WithNonDefaultCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.Create(ContentTypeId);
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DefaultLyricsCategorySpecification_WithInactiveDefaultLyricsCategory_ShouldReturnFalse()
    {
        // Arrange
        CategoryEntity category = CategoryFactory.CreateDefaultForLyrics(ContentTypeId);
        category.Deactivate();
        var spec = new DefaultLyricsCategorySpecification();

        // Act
        bool result = spec.IsSatisfiedBy(category);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
